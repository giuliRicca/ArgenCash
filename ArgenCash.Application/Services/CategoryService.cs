using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CategoryDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);

        return categories
            .Where(c => c.IsSystem || c.UserId == userId)
            .Select(Map)
            .ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        return category is null ? null : Map(category);
    }

    public async Task<Guid> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var transactionType = TransactionTypes.ToEnum(request.Type);

        var category = Category.CreateUserCategory(request.Name, transactionType, userId);

        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        return category.Id;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return false;

        if (category.IsSystem)
            return false;

        if (category.UserId != userId)
            return false;

        await _repository.DeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync();

        return true;
    }

    private static CategoryDto Map(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = TransactionTypes.ToString(category.Type),
            IsSystem = category.IsSystem,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }
}
