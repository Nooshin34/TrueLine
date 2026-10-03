using TrueLine.Application.Abstractions;
using TrueLine.Application.Common;
using TrueLine.Application.Stories;
using TrueLine.Domain.Entities;

namespace TrueLine.Application.Auth;

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

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenIssuer _tokens;
    private readonly INewsImageStorage _images;
    private readonly ICurrentUser _current;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwords,
        ITokenIssuer tokens,
        INewsImageStorage images,
        ICurrentUser current)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _images = images;
        _current = current;
    }

    public async Task<ServiceResult<AuthSession>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var name = command.Name.Trim();

        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            return ServiceResult<AuthSession>.Fail(ServiceError.Conflict, "An account with this email already exists.");
        }

        var user = new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _passwords.Hash(user, command.Password);

        _users.Add(user);
        await _users.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthSession>.Ok(SessionFor(user));
    }

    public async Task<ServiceResult<AuthSession>> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !_passwords.Verify(user, user.PasswordHash, command.Password))
        {
            return ServiceResult<AuthSession>.Fail(ServiceError.Unauthorized, "Email or password is incorrect.");
        }

        return ServiceResult<AuthSession>.Ok(SessionFor(user));
    }

    public async Task<ServiceResult<NewsFile>> OpenAvatarAsync(CancellationToken cancellationToken)
    {
        var user = await CurrentUserAsync(cancellationToken);
        if (user?.AvatarObjectKey is null)
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        var stored = await _images.OpenAsync(user.AvatarObjectKey, cancellationToken);
        if (stored is null)
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        return ServiceResult<NewsFile>.Ok(new NewsFile
        {
            Content = stored.Content,
            ContentType = user.AvatarContentType ?? "application/octet-stream",
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

            var user = await CurrentUserAsync(cancellationToken);
            if (user is null)
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
            var objectKey = $"avatars/{user.Id}/{Guid.NewGuid():N}{extension}";

            try
            {
                await _images.UploadObjectAsync(objectKey, file.Content, file.ContentType, cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult.Fail(ServiceError.StorageFailed, ImageStoreError);
            }

            var previousKey = user.AvatarObjectKey;
            user.AvatarObjectKey = objectKey;
            user.AvatarContentType = file.ContentType;
            await _users.SaveChangesAsync(cancellationToken);

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

    private async Task<User?> CurrentUserAsync(CancellationToken cancellationToken)
    {
        if (_current.Id is null)
        {
            return null;
        }

        return await _users.FindByIdAsync(_current.Id.Value, cancellationToken);
    }

    private AuthSession SessionFor(User user) => new()
    {
        Token = _tokens.Create(user),
        Name = user.Name,
        Email = user.Email,
        HasAvatar = user.AvatarObjectKey is not null,
    };
}
