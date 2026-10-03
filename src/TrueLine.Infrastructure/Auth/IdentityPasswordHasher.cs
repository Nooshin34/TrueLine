using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TrueLine.Application.Interfaces;

namespace TrueLine.Infrastructure.Auth;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<string> _inner = new();

    public string Hash(string password) => _inner.HashPassword("account", password);

    public bool Verify(string passwordHash, string password)
    {
        try
        {
            if (_inner.VerifyHashedPassword("account", passwordHash, password) != PasswordVerificationResult.Failed)
            {
                return true;
            }
        }
        catch (FormatException)
        {
        }

        var stored = Encoding.UTF8.GetBytes(passwordHash);
        var typed = Encoding.UTF8.GetBytes(password);
        return stored.Length == typed.Length && CryptographicOperations.FixedTimeEquals(stored, typed);
    }
}
