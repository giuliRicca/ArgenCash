namespace ArgenCash.Domain.Entities;

public class LearnedCategoryMapping
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string NormalizedKey { get; private set; } = string.Empty;
    public TransactionType TransactionType { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private LearnedCategoryMapping() { }

    public static LearnedCategoryMapping Create(Guid userId, string normalizedKey, TransactionType transactionType, Guid categoryId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(normalizedKey))
            throw new ArgumentException("Normalized key is required.", nameof(normalizedKey));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));

        var now = DateTime.UtcNow;
        return new LearnedCategoryMapping
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NormalizedKey = NormalizeKey(normalizedKey),
            TransactionType = transactionType,
            CategoryId = categoryId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void UpdateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));

        CategoryId = categoryId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static string NormalizeKey(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
