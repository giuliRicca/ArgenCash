namespace ArgenCash.Domain.Entities;

public class Budget
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Budget() { }

    public static Budget Create(Guid userId, Guid categoryId, decimal amount, string currency)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = EnsureRequiredId(categoryId, nameof(categoryId), "Category id is required."),
            Amount = NormalizeAmount(amount),
            Currency = NormalizeCurrencyCode(currency, nameof(currency)),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(Guid categoryId, decimal amount, string currency)
    {
        CategoryId = EnsureRequiredId(categoryId, nameof(categoryId), "Category id is required.");
        Amount = NormalizeAmount(amount);
        Currency = NormalizeCurrencyCode(currency, nameof(currency));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static Guid EnsureRequiredId(Guid value, string parameterName, string message)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message, parameterName);
        }

        return value;
    }

    private static decimal NormalizeAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Budget amount must be greater than zero.", nameof(amount));
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCurrencyCode(string currencyCode, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Budget currency is required.", parameterName);
        }

        var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();

        if (normalizedCurrencyCode is not ("USD" or "ARS"))
        {
            throw new ArgumentException("Budget currency must be USD or ARS.", parameterName);
        }

        return normalizedCurrencyCode;
    }
}
