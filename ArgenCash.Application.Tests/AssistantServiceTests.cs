using ArgenCash.Application.DTOs.Accounts.Responses;
using ArgenCash.Application.DTOs.Accounts.Requests;
using ArgenCash.Application.DTOs.Accounts.Snapshots;
using ArgenCash.Application.DTOs.Assistant.Requests;
using ArgenCash.Application.DTOs.Assistant.Responses;
using ArgenCash.Application.DTOs.Budgets.Snapshots;
using ArgenCash.Application.DTOs.Transactions.Requests;
using ArgenCash.Application.DTOs.Transactions.Responses;
using ArgenCash.Application.Interfaces;
using ArgenCash.Application.Services;
using ArgenCash.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArgenCash.Application.Tests;

[TestClass]
public class AssistantServiceTests
{
    [TestMethod]
    public async Task CreateTransactionDraftAsync_ParsesArgentineExpensePhrase()
    {
        var mercadoPagoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var foodCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var service = CreateService([
            new AccountBalanceSnapshot { Id = mercadoPagoId, Name = "Mercado Pago", CurrencyCode = "ARS" }
        ]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest { Text = "gasté 15 lucas en café con mercado pago ayer" });

        Assert.AreEqual("ready_to_confirm", response.State);
        Assert.IsNotNull(response.Draft);
        Assert.AreEqual(15000m, response.Draft.Amount);
        Assert.AreEqual("ARS", response.Draft.Currency);
        Assert.AreEqual(TransactionTypes.Expense, response.Draft.TransactionType);
        Assert.AreEqual(mercadoPagoId, response.Draft.AccountId);
        Assert.AreEqual(foodCategoryId, response.Draft.CategoryId);
        Assert.AreEqual(DateTime.UtcNow.Date.AddDays(-1).AddHours(12), response.Draft.TransactionDate);
    }

    [TestMethod]
    public async Task CreateTransactionDraftAsync_ParsesMilMultiplier()
    {
        var accountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var service = CreateService([
            new AccountBalanceSnapshot { Id = accountId, Name = "ARQ Checking", CurrencyCode = "ARS" }
        ]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest { Text = "gaste 35 mil pesos en internet de starlink este mes con arq checking" });

        Assert.IsNotNull(response.Draft);
        Assert.AreEqual(35000m, response.Draft.Amount);
        Assert.AreEqual("ARS", response.Draft.Currency);
        Assert.AreEqual(accountId, response.Draft.AccountId);
    }

    [TestMethod]
    public async Task CreateTransactionDraftAsync_MarksTransferUnsupported()
    {
        var service = CreateService([]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest { Text = "pasé 100 lucas de banco a mp" });

        Assert.AreEqual("unsupported", response.State);
        Assert.IsTrue(response.Message?.Contains("transferencias", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task CreateTransactionDraftAsync_MarksInstallmentsUnsupported()
    {
        var service = CreateService([]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest { Text = "compré zapatillas en 3 cuotas con visa" });

        Assert.AreEqual("unsupported", response.State);
        Assert.IsTrue(response.Message?.Contains("cuotas", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task CreateTransactionDraftAsync_MarksCreditCardSettlementUnsupported()
    {
        var service = CreateService([]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest { Text = "pagué la visa" });

        Assert.AreEqual("unsupported", response.State);
        Assert.IsTrue(response.Message?.Contains("resumen", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task CreateTransactionDraftAsync_DoesNotAskCategoryAgainWhenUncategorizedAccepted()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var service = CreateService([
            new AccountBalanceSnapshot { Id = accountId, Name = "Banco", CurrencyCode = "ARS" }
        ]);

        var response = await service.CreateTransactionDraftAsync(
            Guid.NewGuid(),
            new TransactionDraftRequest
            {
                Text = "Sin categorizar",
                PreviousDraft = new()
                {
                    AccountId = accountId,
                    Amount = 300000m,
                    Currency = "ARS",
                    TransactionType = TransactionTypes.Income,
                    Description = "Sueldo",
                    CategoryId = null,
                    CategorySkipped = true,
                    TransactionDate = DateTime.UtcNow.Date.AddHours(12)
                }
            });

        Assert.AreEqual("ready_to_confirm", response.State);
        Assert.IsNull(response.FollowUp);
        Assert.IsNotNull(response.Draft);
        Assert.IsTrue(response.Draft.CategorySkipped);
        Assert.IsNull(response.Draft.CategoryId);
    }

    [TestMethod]
    public async Task HandleChatAsync_SavesTerseExpenseWithDefaultExpenseAccount()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accountRepository = new FakeAccountRepository([
            new AccountBalanceSnapshot { Id = accountId, Name = "Mercado Pago", CurrencyCode = "ARS" }
        ]);
        var accountService = new FakeAccountService();
        var service = CreateService(accountRepository, accountService, defaultExpenseAccountId: accountId);

        var response = await service.HandleChatAsync(userId, new AssistantChatRequest { Text = "4500 cafe" });

        Assert.AreEqual(AssistantChatResponseTypes.TransactionSaved, response.Type);
        Assert.IsNotNull(response.Transaction);
        Assert.AreEqual(accountId, response.Transaction.AccountId);
        Assert.AreEqual(4500m, response.Transaction.Amount);
        Assert.AreEqual(TransactionTypes.Expense, response.Transaction.TransactionType);
        Assert.AreEqual(TransactionSources.AssistantText, response.Transaction.Source);
        Assert.AreEqual("4500 cafe", accountService.LastRequest?.AssistantRawInput);
    }

    [TestMethod]
    public async Task HandleChatAsync_SavesSignedIncomeWithDefaultIncomeAccount()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var accountService = new FakeAccountService();
        var service = CreateService(
            new FakeAccountRepository([new AccountBalanceSnapshot { Id = accountId, Name = "Banco", CurrencyCode = "ARS" }]),
            accountService,
            defaultIncomeAccountId: accountId);

        var response = await service.HandleChatAsync(userId, new AssistantChatRequest { Text = "+300k sueldo" });

        Assert.AreEqual(AssistantChatResponseTypes.TransactionSaved, response.Type);
        Assert.IsNotNull(response.Transaction);
        Assert.AreEqual(TransactionTypes.Income, response.Transaction.TransactionType);
        Assert.AreEqual(300000m, response.Transaction.Amount);
        Assert.AreEqual(accountId, response.Transaction.AccountId);
    }

    [TestMethod]
    public async Task HandleChatAsync_AsksForAmountWhenMissing()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var service = CreateService(
            new FakeAccountRepository([new AccountBalanceSnapshot { Id = accountId, Name = "Mercado Pago", CurrencyCode = "ARS" }]),
            new FakeAccountService(),
            defaultExpenseAccountId: accountId);

        var response = await service.HandleChatAsync(Guid.NewGuid(), new AssistantChatRequest { Text = "cafe" });

        Assert.AreEqual(AssistantChatResponseTypes.NeedsFollowUp, response.Type);
        Assert.AreEqual("amount", response.FollowUp?.Field);
    }

    [TestMethod]
    public async Task HandleChatAsync_AsksForAccountWhenDefaultMissing()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var service = CreateService(
            new FakeAccountRepository([new AccountBalanceSnapshot { Id = accountId, Name = "Mercado Pago", CurrencyCode = "ARS" }]),
            new FakeAccountService());

        var response = await service.HandleChatAsync(Guid.NewGuid(), new AssistantChatRequest { Text = "4500 cafe" });

        Assert.AreEqual(AssistantChatResponseTypes.NeedsFollowUp, response.Type);
        Assert.AreEqual("accountId", response.FollowUp?.Field);
    }

    [TestMethod]
    public async Task HandleChatAsync_ReturnsDuplicateWarningWithoutSaving()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accountRepository = new FakeAccountRepository([
            new AccountBalanceSnapshot { Id = accountId, Name = "Mercado Pago", CurrencyCode = "ARS" }
        ])
        {
            HasDuplicate = true
        };
        var accountService = new FakeAccountService();
        var service = CreateService(accountRepository, accountService, defaultExpenseAccountId: accountId);

        var response = await service.HandleChatAsync(Guid.NewGuid(), new AssistantChatRequest { Text = "4500 cafe" });

        Assert.AreEqual(AssistantChatResponseTypes.DuplicateWarning, response.Type);
        Assert.IsNull(accountService.LastRequest);
    }

    [TestMethod]
    public async Task HandleChatAsync_PreservesIncomeTypeWhenAccountFollowUpAnswered()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var accountService = new FakeAccountService();
        var service = CreateService(
            new FakeAccountRepository([new AccountBalanceSnapshot { Id = accountId, Name = "Banco", CurrencyCode = "USD" }]),
            accountService);

        var followUpResponse = await service.HandleChatAsync(userId, new AssistantChatRequest { Text = "cobre 2650 dolares" });
        Assert.AreEqual(AssistantChatResponseTypes.NeedsFollowUp, followUpResponse.Type);
        Assert.AreEqual(TransactionTypes.Income, followUpResponse.Draft?.TransactionType);

        var selectedDraft = followUpResponse.Draft!;
        selectedDraft.AccountId = accountId;

        var savedResponse = await service.HandleChatAsync(
            userId,
            new AssistantChatRequest
            {
                Text = "Banco",
                PreviousDraft = selectedDraft
            });

        Assert.AreEqual(AssistantChatResponseTypes.TransactionSaved, savedResponse.Type);
        Assert.AreEqual(TransactionTypes.Income, savedResponse.Transaction?.TransactionType);
        Assert.AreEqual(TransactionTypes.Income, accountService.LastRequest?.TransactionType);
        Assert.AreEqual(2650m, accountService.LastRequest?.Amount);
        Assert.AreEqual("USD", accountService.LastRequest?.Currency);
    }

    private static AssistantService CreateService(IReadOnlyList<AccountBalanceSnapshot> accounts)
    {
        return CreateService(new FakeAccountRepository(accounts), new FakeAccountService());
    }

    private static AssistantService CreateService(
        FakeAccountRepository accountRepository,
        FakeAccountService accountService,
        Guid? defaultExpenseAccountId = null,
        Guid? defaultIncomeAccountId = null)
    {
        return new AssistantService(
            accountRepository,
            new FakeCategoryRepository(),
            new FakeLearnedCategoryMappingRepository(),
            new FakeAssistantPreferencesRepository(defaultExpenseAccountId, defaultIncomeAccountId),
            accountService);
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        private readonly IReadOnlyList<AccountBalanceSnapshot> _accounts;

        public FakeAccountRepository(IReadOnlyList<AccountBalanceSnapshot> accounts)
        {
            _accounts = accounts;
        }

        public Task AddAsync(Account account) => throw new NotImplementedException();
        public Task<Account?> GetForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddTransactionAsync(Transaction transaction) => throw new NotImplementedException();
        public Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Transaction>> GetTransactionsByTransferGroupIdAsync(Guid transferGroupId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId, int transactionLimit = 50, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(_accounts.AsEnumerable());
        public Task<PagedResultDto<DashboardRecentTransactionDto>> GetRecentTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<MonthlyTransactionSummaryDto> GetMonthlyTransactionSummaryAsync(Guid userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public bool HasDuplicate { get; init; }
        public Task<bool> HasDuplicateTransactionAsync(Guid userId, Guid accountId, decimal amount, TransactionType transactionType, Guid? categoryId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default) => Task.FromResult(HasDuplicate);
        public Task<List<CreditAccountSettlementCandidateSnapshot>> GetCreditSettlementCandidatesAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<decimal> GetCreditStatementNetExpenseAsync(Guid creditAccountId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveChangesAsync() => throw new NotImplementedException();
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories =
        [
            Category.CreateSystemCategory(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Food", TransactionType.Expense, DateTime.UtcNow),
            Category.CreateSystemCategory(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Salary", TransactionType.Income, DateTime.UtcNow)
        ];

        public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_categories);
        public Task<List<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default) => Task.FromResult(_categories);
        public Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Category>());
        public Task<List<Category>> GetVisibleForUserAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(_categories);
        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_categories.FirstOrDefault(category => category.Id == id));
        public Task AddAsync(Category category) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveChangesAsync() => throw new NotImplementedException();
    }

    private sealed class FakeLearnedCategoryMappingRepository : ILearnedCategoryMappingRepository
    {
        public Task<LearnedCategoryMapping?> GetAsync(Guid userId, string normalizedKey, TransactionType transactionType, CancellationToken cancellationToken = default) => Task.FromResult<LearnedCategoryMapping?>(null);
        public Task AddAsync(LearnedCategoryMapping mapping, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeAssistantPreferencesRepository : IAssistantPreferencesRepository
    {
        private readonly Guid? _defaultExpenseAccountId;
        private readonly Guid? _defaultIncomeAccountId;

        public FakeAssistantPreferencesRepository(Guid? defaultExpenseAccountId, Guid? defaultIncomeAccountId)
        {
            _defaultExpenseAccountId = defaultExpenseAccountId;
            _defaultIncomeAccountId = defaultIncomeAccountId;
        }

        public Task<AssistantPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var preferences = AssistantPreferences.Create(userId);
            preferences.SetDefaultAccount(TransactionType.Expense, _defaultExpenseAccountId);
            preferences.SetDefaultAccount(TransactionType.Income, _defaultIncomeAccountId);
            return Task.FromResult<AssistantPreferences?>(preferences);
        }

        public Task AddAsync(AssistantPreferences preferences, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeAccountService : IAccountService
    {
        public CreateTransactionRequest? LastRequest { get; private set; }
        public Guid CreatedTransactionId { get; } = Guid.Parse("99999999-9999-9999-9999-999999999999");

        public Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request) => throw new NotImplementedException();
        public Task<bool> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Guid> CreateTransactionAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(CreatedTransactionId);
        }

        public Task<bool> UpdateTransactionAsync(Guid transactionId, Guid userId, UpdateTransactionRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Guid> CreateTransferAsync(Guid userId, CreateTransferRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AccountDto?> GetAccountByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AccountDetailDto?> GetAccountDetailByIdAsync(Guid id, Guid userId, int transactionLimit = 50, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResultDto<DashboardRecentTransactionDto>> GetRecentTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<MonthlyTransactionSummaryDto> GetMonthlyTransactionSummaryAsync(Guid userId, int? month = null, int? year = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
