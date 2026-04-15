using System.Text.Json.Serialization;

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

    public static TransactionType ToEnum(string transactionType)
    {
        return transactionType.ToUpperInvariant() switch
        {
            Income => TransactionType.Income,
            Expense => TransactionType.Expense,
            _ => throw new ArgumentException($"Invalid transaction type: {transactionType}")
        };
    }

    public static string ToString(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Income => Income,
            TransactionType.Expense => Expense,
            _ => throw new ArgumentException($"Invalid transaction type enum: {transactionType}")
        };
    }
}
