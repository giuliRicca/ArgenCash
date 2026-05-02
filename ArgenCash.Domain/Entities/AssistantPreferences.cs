namespace ArgenCash.Domain.Entities;

public class AssistantPreferences
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? DefaultExpenseAccountId { get; private set; }
    public Guid? DefaultIncomeAccountId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private AssistantPreferences() { }

    public static AssistantPreferences Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var now = DateTime.UtcNow;
        return new AssistantPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public Guid? GetDefaultAccountId(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Expense => DefaultExpenseAccountId,
            TransactionType.Income => DefaultIncomeAccountId,
            _ => throw new ArgumentException($"Invalid transaction type enum: {transactionType}", nameof(transactionType))
        };
    }

    public void SetDefaultAccount(TransactionType transactionType, Guid? accountId)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        }

        switch (transactionType)
        {
            case TransactionType.Expense:
                DefaultExpenseAccountId = accountId;
                break;
            case TransactionType.Income:
                DefaultIncomeAccountId = accountId;
                break;
            default:
                throw new ArgumentException($"Invalid transaction type enum: {transactionType}", nameof(transactionType));
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }
}
