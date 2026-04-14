
namespace ArgenCash.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
