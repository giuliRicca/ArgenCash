namespace ArgenCash.Application.DTOs.Accounts.Responses;

/// <summary>
/// Describes the result of processing due credit card settlements.
/// </summary>
public class CreditSettlementProcessResultDto
{
    /// <summary>
    /// Gets the number of settlement transfers created.
    /// </summary>
    public int ProcessedCount { get; init; }

    /// <summary>
    /// Gets the number of credit accounts skipped.
    /// </summary>
    public int SkippedCount { get; init; }
}
