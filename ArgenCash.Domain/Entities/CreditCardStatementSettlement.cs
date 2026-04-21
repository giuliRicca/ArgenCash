namespace ArgenCash.Domain.Entities;

/// <summary>
/// Represents a generated payment for a credit account monthly statement.
/// </summary>
public class CreditCardStatementSettlement
{
    /// <summary>
    /// Gets the settlement identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the credit account identifier.
    /// </summary>
    public Guid CreditAccountId { get; private set; }

    /// <summary>
    /// Gets the funding account identifier.
    /// </summary>
    public Guid FundingAccountId { get; private set; }

    /// <summary>
    /// Gets the statement year.
    /// </summary>
    public int StatementYear { get; private set; }

    /// <summary>
    /// Gets the statement month.
    /// </summary>
    public int StatementMonth { get; private set; }

    /// <summary>
    /// Gets the settlement amount in account currency.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the settlement currency code.
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transfer group id created for settlement payment.
    /// </summary>
    public Guid TransferGroupId { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    private CreditCardStatementSettlement()
    {
    }

    /// <summary>
    /// Creates a settlement record.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="creditAccountId">Credit account identifier.</param>
    /// <param name="fundingAccountId">Funding account identifier.</param>
    /// <param name="statementYear">Statement year.</param>
    /// <param name="statementMonth">Statement month.</param>
    /// <param name="amount">Settlement amount in account currency.</param>
    /// <param name="currency">Settlement currency code.</param>
    /// <param name="transferGroupId">Associated transfer group identifier.</param>
    /// <returns>A validated settlement entity.</returns>
    public static CreditCardStatementSettlement Create(
        Guid userId,
        Guid creditAccountId,
        Guid fundingAccountId,
        int statementYear,
        int statementMonth,
        decimal amount,
        string currency,
        Guid transferGroupId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (creditAccountId == Guid.Empty)
        {
            throw new ArgumentException("Credit account id is required.", nameof(creditAccountId));
        }

        if (fundingAccountId == Guid.Empty)
        {
            throw new ArgumentException("Funding account id is required.", nameof(fundingAccountId));
        }

        if (creditAccountId == fundingAccountId)
        {
            throw new ArgumentException("Funding account must be different from the credit account.", nameof(fundingAccountId));
        }

        if (statementYear < 2000 || statementYear > 3000)
        {
            throw new ArgumentOutOfRangeException(nameof(statementYear), "Statement year is out of range.");
        }

        if (statementMonth < 1 || statementMonth > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(statementMonth), "Statement month must be between 1 and 12.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Settlement amount must be greater than zero.", nameof(amount));
        }

        var normalizedCurrency = NormalizeCurrencyCode(currency, nameof(currency));

        if (transferGroupId == Guid.Empty)
        {
            throw new ArgumentException("Transfer group id is required.", nameof(transferGroupId));
        }

        return new CreditCardStatementSettlement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreditAccountId = creditAccountId,
            FundingAccountId = fundingAccountId,
            StatementYear = statementYear,
            StatementMonth = statementMonth,
            Amount = amount,
            Currency = normalizedCurrency,
            TransferGroupId = transferGroupId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static string NormalizeCurrencyCode(string currencyCode, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("Currency code is required.", parameterName);
        }

        var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();

        if (normalizedCurrencyCode.Length != 3 || !normalizedCurrencyCode.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", parameterName);
        }

        return normalizedCurrencyCode;
    }
}
