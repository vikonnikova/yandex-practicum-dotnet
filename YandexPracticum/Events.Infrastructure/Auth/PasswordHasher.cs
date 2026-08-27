using Events.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Events.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object DummyUser = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(DummyUser, password);
    }

    public bool Verify(string inputPassword, string storedHash)
    {
        var result = _hasher.VerifyHashedPassword(DummyUser, storedHash, inputPassword);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}