namespace ArgenCash.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public bool IsSystem { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Category() { }

    public static Category CreateSystemCategory(string name, TransactionType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            IsSystem = true,
            UserId = null,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Category CreateUserCategory(string name, TransactionType type, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            IsSystem = false,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Category CreateSystemCategory(Guid id, string name, TransactionType type, DateTime createdAtUtc)
    {
        return new Category
        {
            Id = id,
            Name = name,
            Type = type,
            IsSystem = true,
            UserId = null,
            CreatedAtUtc = createdAtUtc
        };
    }
}
