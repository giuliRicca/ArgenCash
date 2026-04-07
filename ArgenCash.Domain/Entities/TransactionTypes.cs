namespace ArgenCash.Domain.Entities;

public static class TransactionTypes
{
    public const string Income = "INCOME";
    public const string Expense = "EXPENSE";

    public static string Normalize(string transactionType, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(transactionType))
        {
            throw new ArgumentException("Transaction type is required.", parameterName);
        }

        var normalizedTransactionType = transactionType.Trim().ToUpperInvariant();

        return normalizedTransactionType switch
        {
            Income => Income,
            Expense => Expense,
            _ => throw new ArgumentException("Transaction type must be INCOME or EXPENSE.", parameterName)
        };
    }
}
