using ArgenCash.Application.DTOs;

namespace ArgenCash.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Guid userId, CreateBudgetRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid budgetId, Guid userId, UpdateBudgetRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid budgetId, Guid userId, CancellationToken cancellationToken = default);
}
