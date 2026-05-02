using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

public class LearnedCategoryMappingRepository : ILearnedCategoryMappingRepository
{
    private readonly ArgenCashDbContext _context;

    public LearnedCategoryMappingRepository(ArgenCashDbContext context)
    {
        _context = context;
    }

    public async Task<LearnedCategoryMapping?> GetAsync(Guid userId, string normalizedKey, TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        var key = LearnedCategoryMapping.NormalizeKey(normalizedKey);
        return await _context.LearnedCategoryMappings
            .SingleOrDefaultAsync(
                mapping => mapping.UserId == userId &&
                           mapping.NormalizedKey == key &&
                           mapping.TransactionType == transactionType,
                cancellationToken);
    }

    public async Task AddAsync(LearnedCategoryMapping mapping, CancellationToken cancellationToken = default)
    {
        await _context.LearnedCategoryMappings.AddAsync(mapping, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
