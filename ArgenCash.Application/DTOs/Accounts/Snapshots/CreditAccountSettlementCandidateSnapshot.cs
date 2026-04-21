using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs.Accounts.Snapshots;

/// <summary>
/// Represents a credit account candidate that may require statement settlement processing.
/// </summary>
public class CreditAccountSettlementCandidateSnapshot
{
    /// <summary>
    /// Gets the account identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the account currency code.
    /// </summary>
    public string CurrencyCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the account exchange rate type used for transfer conversion.
    /// </summary>
    public ExchangeRateType ExchangeRateType { get; init; }

    /// <summary>
    /// Gets the funding account identifier.
    /// </summary>
    public Guid FundingAccountId { get; init; }

    /// <summary>
    /// Gets the payment day of month.
    /// </summary>
    public int PaymentDayOfMonth { get; init; }
}
