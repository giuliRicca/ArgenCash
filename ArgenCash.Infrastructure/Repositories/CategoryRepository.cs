using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ArgenCashDbContext _context;

    public CategoryRepository(ArgenCashDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.IsSystem)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetVisibleForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.IsSystem || category.UserId == userId)
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (category is not null)
        {
            _context.Categories.Remove(category);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
