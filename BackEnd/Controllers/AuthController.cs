using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueLine.Application.Services;
using TrueLine.Domain.Dto;

namespace TrueLine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.RegisterAsync(new RegisterCommand
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
        }, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
        }, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [Authorize]
    [HttpGet("avatar")]
    public async Task<IActionResult> GetAvatar(CancellationToken cancellationToken)
    {
        var result = await _auth.OpenAvatarAsync(cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return File(result.Value.Content, result.Value.ContentType);
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 3 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file, CancellationToken cancellationToken)
    {
        var result = await _auth.UploadAvatarAsync(ToFile(file), cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToActionResult(result);
        }

        return NoContent();
    }

    private static AuthResponse ToResponse(AuthSession session) => new()
    {
        Token = session.Token,
        Name = session.Name,
        Email = session.Email,
        HasAvatar = session.HasAvatar,
        Role = session.Role,
    };

    private static IncomingFile? ToFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        return new IncomingFile
        {
            Content = file.OpenReadStream(),
            Length = file.Length,
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
    }
}
