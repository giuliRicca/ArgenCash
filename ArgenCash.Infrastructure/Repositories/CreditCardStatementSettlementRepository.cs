using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

/// <summary>
/// Stores and queries settlement records.
/// </summary>
public class CreditCardStatementSettlementRepository(ArgenCashDbContext context) : ICreditCardStatementSettlementRepository
{
    private readonly ArgenCashDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid creditAccountId,
        int statementYear,
        int statementMonth,
        CancellationToken cancellationToken = default)
    {
        return await _context.CreditCardStatementSettlements
            .AsNoTracking()
            .AnyAsync(
                settlement =>
                    settlement.CreditAccountId == creditAccountId &&
                    settlement.StatementYear == statementYear &&
                    settlement.StatementMonth == statementMonth,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CreditCardStatementSettlement settlement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settlement);
        await _context.CreditCardStatementSettlements.AddAsync(settlement, cancellationToken);
    }
}
