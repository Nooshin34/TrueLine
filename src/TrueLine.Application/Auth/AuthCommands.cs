namespace TrueLine.Application.Auth;

public sealed class RegisterCommand
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}

public sealed class LoginCommand
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}

public sealed class AuthSession
{
    public required string Token { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public bool HasAvatar { get; init; }

    public required string Role { get; init; }
}
