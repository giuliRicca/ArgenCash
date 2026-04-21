namespace ArgenCash.Application.Interfaces;

/// <summary>
/// Processes due credit card statement settlements.
/// </summary>
public interface ICreditCardSettlementService
{
    /// <summary>
    /// Processes due credit settlements for the specified user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Settlement processing summary.</returns>
    Task<CreditSettlementProcessResultDto> ProcessDueSettlementsAsync(Guid userId, CancellationToken cancellationToken = default);
}
