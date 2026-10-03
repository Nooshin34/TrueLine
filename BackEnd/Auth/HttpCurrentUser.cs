using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrueLine.Application.Abstractions;

namespace TrueLine.Api.Auth;

public sealed class HttpCurrentUser : ICurrentUser
{
    public HttpCurrentUser(IHttpContextAccessor http)
    {
        var user = http.HttpContext?.User;
        var value = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        Id = int.TryParse(value, out var id) ? id : null;
    }

    public int? Id { get; }
}
