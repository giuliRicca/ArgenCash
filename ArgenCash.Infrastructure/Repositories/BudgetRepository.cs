using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ArgenCashDbContext _context;

    public BudgetRepository(ArgenCashDbContext context)
    {
        _context = context;
    }

    public async Task<List<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Where(budget => budget.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .SingleOrDefaultAsync(budget => budget.Id == id, cancellationToken);
    }

    public async Task<Budget?> GetByCategoryIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .SingleOrDefaultAsync(
                budget => budget.UserId == userId && budget.CategoryId == categoryId,
                cancellationToken);
    }

    public async Task<List<CategoryExpenseTotalSnapshot>> GetExpenseTotalsByCategoryAsync(
        Guid userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken cancellationToken = default)
    {
        var userAccounts = _context.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .Select(account => account.Id);

        return await _context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.TransactionType == TransactionType.Expense &&
                transaction.CategoryId.HasValue &&
                transaction.TransactionDate >= fromUtc &&
                transaction.TransactionDate < toUtcExclusive &&
                userAccounts.Contains(transaction.AccountId))
            .GroupBy(transaction => transaction.CategoryId!.Value)
            .Select(group => new CategoryExpenseTotalSnapshot
            {
                CategoryId = group.Key,
                TotalUsd = group.Sum(transaction => transaction.ConvertedAmountUSD),
                TotalArs = group.Sum(transaction => transaction.ConvertedAmountARS)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        await _context.Budgets.AddAsync(budget, cancellationToken);
    }

    public Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Remove(budget);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
