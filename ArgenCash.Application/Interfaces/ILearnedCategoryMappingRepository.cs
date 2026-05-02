using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface ILearnedCategoryMappingRepository
{
    Task<LearnedCategoryMapping?> GetAsync(Guid userId, string normalizedKey, TransactionType transactionType, CancellationToken cancellationToken = default);
    Task AddAsync(LearnedCategoryMapping mapping, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
