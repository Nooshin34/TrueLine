using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrueLine.Api.Contracts;
using TrueLine.Api.Data;
using TrueLine.Api.Storage;
using TrueLine.Backend.Entities;

namespace TrueLine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedAvatarTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwords;
    private readonly IConfiguration _configuration;
    private readonly INewsImageStorage _images;

    public AuthController(
        AppDbContext db,
        IPasswordHasher<User> passwords,
        IConfiguration configuration,
        INewsImageStorage images)
    {
        _db = db;
        _passwords = passwords;
        _configuration = configuration;
        _images = images;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();

        var emailTaken = await _db.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (emailTaken)
        {
            return Conflict("An account with this email already exists.");
        }

        var user = new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = _passwords.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(CreateResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null)
        {
            return Unauthorized("Email or password is incorrect.");
        }

        var result = _passwords.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Email or password is incorrect.");
        }

        return Ok(CreateResponse(user));
    }

    [Authorize]
    [HttpGet("avatar")]
    public async Task<IActionResult> GetAvatar(CancellationToken cancellationToken)
    {
        var user = await CurrentUserAsync(cancellationToken);
        if (user?.AvatarObjectKey is null)
        {
            return NotFound();
        }

        var stored = await _images.OpenAsync(user.AvatarObjectKey, cancellationToken);
        if (stored is null)
        {
            return NotFound();
        }

        return File(stored.Content, user.AvatarContentType ?? "application/octet-stream");
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 3 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > MaxAvatarBytes || !AllowedAvatarTypes.Contains(file.ContentType))
        {
            return BadRequest("Photo must be a JPG, PNG, WEBP, or GIF no larger than 2 MB.");
        }

        var user = await CurrentUserAsync(cancellationToken);
        if (user is null)
        {
            return Unauthorized();
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
            await using var stream = file.OpenReadStream();
            await _images.UploadObjectAsync(objectKey, stream, file.ContentType, cancellationToken);
        }
        catch (Exception)
        {
            return Problem(
                detail: "Could not store the image. Check that MinIO is running and the MinIO settings are filled in.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var previousKey = user.AvatarObjectKey;
        user.AvatarObjectKey = objectKey;
        user.AvatarContentType = file.ContentType;
        await _db.SaveChangesAsync(cancellationToken);

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

        return NoContent();
    }

    private async Task<User?> CurrentUserAsync(CancellationToken cancellationToken)
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id))
        {
            return null;
        }

        return await _db.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private AuthResponse CreateResponse(User user)
    {
        var jwt = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
            ],
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Name = user.Name,
            Email = user.Email,
            HasAvatar = user.AvatarObjectKey is not null,
        };
    }
}
