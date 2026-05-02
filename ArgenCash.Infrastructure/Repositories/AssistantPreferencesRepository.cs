using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories;

public class AssistantPreferencesRepository(ArgenCashDbContext context) : IAssistantPreferencesRepository
{
    private readonly ArgenCashDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<AssistantPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AssistantPreferences
            .SingleOrDefaultAsync(preferences => preferences.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(AssistantPreferences preferences, CancellationToken cancellationToken = default)
    {
        await _context.AssistantPreferences.AddAsync(preferences, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
