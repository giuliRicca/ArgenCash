using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ArgenCash.Infrastructure.Persistence;

public class ArgenCashDbContext : DbContext
{
    public ArgenCashDbContext(DbContextOptions<ArgenCashDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<CreditCardStatementSettlement> CreditCardStatementSettlements => Set<CreditCardStatementSettlement>();
    public DbSet<LearnedCategoryMapping> LearnedCategoryMappings => Set<LearnedCategoryMapping>();
    public DbSet<AssistantPreferences> AssistantPreferences => Set<AssistantPreferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SeedSystemCategories(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void SeedSystemCategories(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            Category.CreateSystemCategory(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Housing", TransactionType.Expense, now),
            Category.CreateSystemCategory(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Utilities", TransactionType.Expense, now),
            Category.CreateSystemCategory(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Food", TransactionType.Expense, now),
            Category.CreateSystemCategory(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Transportation", TransactionType.Expense, now),
            Category.CreateSystemCategory(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Insurance", TransactionType.Expense, now),
            Category.CreateSystemCategory(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Salary", TransactionType.Income, now),
            Category.CreateSystemCategory(Guid.Parse("77777777-7777-7777-7777-777777777777"), "Freelance Work", TransactionType.Income, now),
            Category.CreateSystemCategory(Guid.Parse("88888888-8888-8888-8888-888888888888"), "Investments", TransactionType.Income, now)
        );
    }
}
