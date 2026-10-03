using Microsoft.AspNetCore.Identity;
using TrueLine.Application.Abstractions;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Auth;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(User user, string password) => _inner.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string password) =>
        _inner.VerifyHashedPassword(user, passwordHash, password) != PasswordVerificationResult.Failed;
}
