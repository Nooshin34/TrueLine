using TrueLine.Application.Auth;
using TrueLine.Application.Interfaces;
using TrueLine.Domain.Dto;
using TrueLine.Domain.Entities;

namespace TrueLine.Application.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthSession>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);

    Task<ServiceResult<AuthSession>> LoginAsync(LoginCommand command, CancellationToken cancellationToken);

    Task<ServiceResult<NewsFile>> OpenAvatarAsync(CancellationToken cancellationToken);

    Task<ServiceResult> UploadAvatarAsync(IncomingFile? file, CancellationToken cancellationToken);
}

public sealed class AuthService : IAuthService
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private const string AvatarTypeError = "Photo must be a JPG, PNG, WEBP, or GIF no larger than 2 MB.";
    private const string ImageStoreError =
        "Could not store the image. Check that MinIO is running and the MinIO settings are filled in.";

    private static readonly HashSet<string> AllowedAvatarTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private readonly IReporterRepository _reporters;
    private readonly IAdminRepository _admins;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenIssuer _tokens;
    private readonly INewsImageStorage _images;
    private readonly ICurrentUser _current;

    public AuthService(
        IReporterRepository reporters,
        IAdminRepository admins,
        IPasswordHasher passwords,
        ITokenIssuer tokens,
        INewsImageStorage images,
        ICurrentUser current)
    {
        _reporters = reporters;
        _admins = admins;
        _passwords = passwords;
        _tokens = tokens;
        _images = images;
        _current = current;
    }

    public async Task<ServiceResult<AuthSession>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var name = command.Name.Trim();

        if (await _reporters.EmailExistsAsync(email, cancellationToken))
        {
            return ServiceResult<AuthSession>.Fail(ServiceError.Conflict, "An account with this email already exists.");
        }

        var reporter = new Reporter
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = _passwords.Hash(command.Password),
        };

        _reporters.Add(reporter);
        await _reporters.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthSession>.Ok(SessionFor(reporter));
    }

    public async Task<ServiceResult<AuthSession>> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var reporter = await _reporters.FindByEmailAsync(email, cancellationToken);
        if (reporter is not null && _passwords.Verify(reporter.PasswordHash, command.Password))
        {
            return ServiceResult<AuthSession>.Ok(SessionFor(reporter));
        }

        var admin = await _admins.FindByEmailAsync(email, cancellationToken);
        if (admin is not null && _passwords.Verify(admin.PasswordHash, command.Password))
        {
            return ServiceResult<AuthSession>.Ok(new AuthSession
            {
                Token = _tokens.Create(admin.Id, admin.Email, admin.Name, AccountRoles.Admin),
                Name = admin.Name,
                Email = admin.Email,
                HasAvatar = false,
                Role = AccountRoles.Admin,
            });
        }

        return ServiceResult<AuthSession>.Fail(ServiceError.Unauthorized, "Email or password is incorrect.");
    }

    public async Task<ServiceResult<NewsFile>> OpenAvatarAsync(CancellationToken cancellationToken)
    {
        var reporter = await CurrentReporterAsync(cancellationToken);
        if (reporter?.AvatarObjectKey is null)
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        var stored = await _images.OpenAsync(reporter.AvatarObjectKey, cancellationToken);
        if (stored is null)
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        return ServiceResult<NewsFile>.Ok(new NewsFile
        {
            Content = stored.Content,
            ContentType = reporter.AvatarContentType ?? "application/octet-stream",
        });
    }

    public async Task<ServiceResult> UploadAvatarAsync(IncomingFile? file, CancellationToken cancellationToken)
    {
        try
        {
            if (file is null || file.Length == 0 || file.Length > MaxAvatarBytes || !AllowedAvatarTypes.Contains(file.ContentType))
            {
                return ServiceResult.Fail(ServiceError.BadRequest, AvatarTypeError);
            }

            var reporter = await CurrentReporterAsync(cancellationToken);
            if (reporter is null)
            {
                return ServiceResult.Fail(ServiceError.Unauthorized);
            }

            var extension = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".bin",
            };
            var objectKey = $"avatars/{reporter.Id}/{Guid.NewGuid():N}{extension}";

            try
            {
                await _images.UploadObjectAsync(objectKey, file.Content, file.ContentType, cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult.Fail(ServiceError.StorageFailed, ImageStoreError);
            }

            var previousKey = reporter.AvatarObjectKey;
            reporter.AvatarObjectKey = objectKey;
            reporter.AvatarContentType = file.ContentType;
            await _reporters.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(previousKey))
            {
                try
                {
                    await _images.DeleteAsync(previousKey, cancellationToken);
                }
                catch (Exception)
                {
                }
            }

            return ServiceResult.Ok();
        }
        finally
        {
            if (file is not null)
            {
                await file.Content.DisposeAsync();
            }
        }
    }

    private async Task<Reporter?> CurrentReporterAsync(CancellationToken cancellationToken)
    {
        if (_current.Id is null || !_current.IsReporter)
        {
            return null;
        }

        return await _reporters.FindByIdAsync(_current.Id.Value, cancellationToken);
    }

    private AuthSession SessionFor(Reporter reporter) => new()
    {
        Token = _tokens.Create(reporter.Id, reporter.Email, reporter.Name, AccountRoles.Reporter),
        Name = reporter.Name,
        Email = reporter.Email,
        HasAvatar = reporter.AvatarObjectKey is not null,
        Role = AccountRoles.Reporter,
    };
}
