using ArgenCash.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ArgenCash.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _passwordHasher = new();
    private static readonly object PasswordOwner = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(PasswordOwner, password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(PasswordOwner, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
