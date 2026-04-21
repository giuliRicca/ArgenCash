using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

/// <summary>
/// Provides persistence operations for credit card statement settlements.
/// </summary>
public interface ICreditCardStatementSettlementRepository
{
    /// <summary>
    /// Determines whether a settlement already exists for the given statement cycle.
    /// </summary>
    /// <param name="creditAccountId">Credit account identifier.</param>
    /// <param name="statementYear">Statement year.</param>
    /// <param name="statementMonth">Statement month.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when settlement exists; otherwise <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(Guid creditAccountId, int statementYear, int statementMonth, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a settlement to the current unit of work.
    /// </summary>
    /// <param name="settlement">Settlement entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task AddAsync(CreditCardStatementSettlement settlement, CancellationToken cancellationToken = default);
}
