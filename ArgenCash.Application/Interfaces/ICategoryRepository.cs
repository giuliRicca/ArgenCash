using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync();
}
