using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrueLine.Application.Abstractions;
using TrueLine.Application.Auth;

namespace TrueLine.Api.Auth;

public sealed class HttpCurrentUser : ICurrentUser
{
    public HttpCurrentUser(IHttpContextAccessor http)
    {
        var principal = http.HttpContext?.User;
        var value = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        Id = int.TryParse(value, out var id) ? id : null;
        IsAdmin = Id is not null && principal?.FindFirstValue("role") == AccountRoles.Admin;
        IsReporter = Id is not null && !IsAdmin;
    }

    public int? Id { get; }

    public bool IsAdmin { get; }

    public bool IsReporter { get; }
}
