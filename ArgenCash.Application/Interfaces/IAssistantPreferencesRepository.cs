using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IAssistantPreferencesRepository
{
    Task<AssistantPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(AssistantPreferences preferences, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
