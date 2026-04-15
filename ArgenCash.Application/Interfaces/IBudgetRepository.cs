using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IBudgetRepository
{
    Task<List<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Budget?> GetByCategoryIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<List<CategoryExpenseTotalSnapshot>> GetExpenseTotalsByCategoryAsync(Guid userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default);
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
