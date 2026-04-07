namespace ArgenCash.Domain.Entities;

public class User
{
    private const int MaxFullNameLength = 150;
    private const int MaxEmailLength = 320;

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string fullName, string email, string passwordHash)
    {
        var normalizedFullName = NormalizeFullName(fullName);
        var normalizedEmail = NormalizeEmail(email);
        var normalizedPasswordHash = NormalizePasswordHash(passwordHash);

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = normalizedFullName,
            Email = normalizedEmail,
            PasswordHash = normalizedPasswordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string NormalizeFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        var normalizedFullName = fullName.Trim();

        if (normalizedFullName.Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.", nameof(fullName));
        }

        return normalizedFullName;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));
        }

        if (!normalizedEmail.Contains('@') || normalizedEmail.StartsWith('@') || normalizedEmail.EndsWith('@'))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        return normalizedEmail;
    }

    private static string NormalizePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        return passwordHash.Trim();
    }
}
