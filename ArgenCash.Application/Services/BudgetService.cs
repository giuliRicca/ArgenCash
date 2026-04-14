using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BudgetService(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<BudgetDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var budgets = await _budgetRepository.GetByUserIdAsync(userId, cancellationToken);
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var monthStartUtc = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStartUtc = monthStartUtc.AddMonths(1);

        var monthlyExpenseTotals = await _budgetRepository.GetExpenseTotalsByCategoryAsync(
            userId,
            monthStartUtc,
            nextMonthStartUtc,
            cancellationToken);

        var totalsByCategoryId = monthlyExpenseTotals.ToDictionary(total => total.CategoryId);
        var categoryNames = categories.ToDictionary(category => category.Id, category => category.Name);

        return budgets
            .OrderBy(budget => categoryNames.GetValueOrDefault(budget.CategoryId, string.Empty))
            .Select(budget => Map(budget, categoryNames, totalsByCategoryId, now.Month, now.Year))
            .ToList();
    }

    public async Task<Guid> CreateAsync(Guid userId, CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await EnsureBudgetCategoryAsync(request.CategoryId, userId, cancellationToken);

        var existingBudget = await _budgetRepository.GetByCategoryIdAsync(userId, category.Id, cancellationToken);
        if (existingBudget is not null)
        {
            throw new ArgumentException("A budget for this category already exists.", nameof(request.CategoryId));
        }

        var budget = Budget.Create(userId, category.Id, request.Amount, request.Currency);

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        return budget.Id;
    }

    public async Task<bool> UpdateAsync(Guid budgetId, Guid userId, UpdateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        if (budget is null || budget.UserId != userId)
        {
            return false;
        }

        var category = await EnsureBudgetCategoryAsync(request.CategoryId, userId, cancellationToken);
        var existingBudget = await _budgetRepository.GetByCategoryIdAsync(userId, category.Id, cancellationToken);
        if (existingBudget is not null && existingBudget.Id != budgetId)
        {
            throw new ArgumentException("A budget for this category already exists.", nameof(request.CategoryId));
        }

        budget.Update(category.Id, request.Amount, request.Currency);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid budgetId, Guid userId, CancellationToken cancellationToken = default)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        if (budget is null || budget.UserId != userId)
        {
            return false;
        }

        await _budgetRepository.DeleteAsync(budget, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Category> EnsureBudgetCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new ArgumentException("Category not found.", nameof(categoryId));
        }

        if (!category.IsSystem && category.UserId != userId)
        {
            throw new ArgumentException("Category does not belong to the current user.", nameof(categoryId));
        }

        if (category.Type != TransactionType.Expense)
        {
            throw new ArgumentException("Budgets can only be assigned to expense categories.", nameof(categoryId));
        }

        return category;
    }

    private static BudgetDto Map(
        Budget budget,
        IReadOnlyDictionary<Guid, string> categoryNames,
        IReadOnlyDictionary<Guid, CategoryExpenseTotalSnapshot> totalsByCategoryId,
        int month,
        int year)
    {
        var hasTotals = totalsByCategoryId.TryGetValue(budget.CategoryId, out var totals);
        var spent = budget.Currency == "ARS"
            ? (hasTotals ? totals!.TotalArs : 0m)
            : (hasTotals ? totals!.TotalUsd : 0m);

        var roundedSpent = decimal.Round(spent, 2, MidpointRounding.AwayFromZero);
        var remaining = decimal.Round(budget.Amount - roundedSpent, 2, MidpointRounding.AwayFromZero);
        var usagePercentage = budget.Amount == 0m
            ? 0m
            : decimal.Round((roundedSpent / budget.Amount) * 100m, 2, MidpointRounding.AwayFromZero);

        return new BudgetDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = categoryNames.GetValueOrDefault(budget.CategoryId, "Unknown category"),
            Amount = budget.Amount,
            Currency = budget.Currency,
            SpentAmount = roundedSpent,
            RemainingAmount = remaining,
            UsagePercentage = usagePercentage,
            Month = month,
            Year = year,
            CreatedAtUtc = budget.CreatedAtUtc,
            UpdatedAtUtc = budget.UpdatedAtUtc
        };
    }
}
