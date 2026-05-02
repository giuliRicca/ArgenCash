using ArgenCash.Application.Exceptions;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IExchangeRateResolver _exchangeRateResolver;
    private readonly ILearnedCategoryMappingRepository _learnedCategoryMappingRepository;

    public AccountService(
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IExchangeRateResolver exchangeRateResolver,
        ILearnedCategoryMappingRepository learnedCategoryMappingRepository)
    {
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _exchangeRateResolver = exchangeRateResolver;
        _learnedCategoryMappingRepository = learnedCategoryMappingRepository;
    }

    public async Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ValidateFundingAccountAsync(
            userId,
            request.AccountType,
            request.FundingAccountId,
            null,
            CancellationToken.None);

        var account = Account.Create(
            request.Name,
            request.CurrencyCode,
            userId,
            request.AccountType,
            request.FundingAccountId,
            request.PaymentDayOfMonth);

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

        return account.Id;
    }

    public async Task<bool> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = await _accountRepository.GetForUpdateAsync(accountId, userId, cancellationToken);

        if (account is null)
        {
            return false;
        }

        var hasNameUpdate = !string.IsNullOrWhiteSpace(request.Name);
        var hasExchangeRateTypeUpdate = request.ExchangeRateType.HasValue;
        var hasAccountTypeUpdate = request.AccountType.HasValue;
        var hasFundingAccountUpdate = request.FundingAccountId.HasValue;
        var hasPaymentDayUpdate = request.PaymentDayOfMonth.HasValue;

        if (!hasNameUpdate &&
            !hasExchangeRateTypeUpdate &&
            !hasAccountTypeUpdate &&
            !hasFundingAccountUpdate &&
            !hasPaymentDayUpdate)
        {
            throw new ArgumentException("At least one account field must be provided.", nameof(request));
        }

        if (hasNameUpdate)
        {
            account.Rename(request.Name!);
        }

        if (hasExchangeRateTypeUpdate)
        {
            account.SetExchangeRateType(request.ExchangeRateType!.Value);
        }

        if (hasAccountTypeUpdate || hasFundingAccountUpdate || hasPaymentDayUpdate)
        {
            var targetAccountType = request.AccountType ?? account.AccountType;
            var targetFundingAccountId = targetAccountType == AccountType.Credit
                ? request.FundingAccountId ?? account.FundingAccountId
                : null;
            var targetPaymentDayOfMonth = targetAccountType == AccountType.Credit
                ? request.PaymentDayOfMonth ?? account.PaymentDayOfMonth
                : null;

            await ValidateFundingAccountAsync(
                userId,
                targetAccountType,
                targetFundingAccountId,
                account.Id,
                cancellationToken);

            account.ConfigureAccountType(targetAccountType, targetFundingAccountId, targetPaymentDayOfMonth);
        }

        await _accountRepository.SaveChangesAsync();

        return true;
    }

    public async Task<Guid> CreateTransactionAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = await _accountRepository.GetForUpdateAsync(request.AccountId, userId, cancellationToken);
        if (account is null)
        {
            throw new ArgumentException("Account not found.", nameof(request.AccountId));
        }

        var normalizedCurrency = NormalizeAssistantCurrency(request.Currency);
        var transactionType = TransactionTypes.ToEnum(request.TransactionType);

        await ValidateCategoryAsync(userId, request.CategoryId, transactionType, cancellationToken);

        var transactionDate = NormalizeTransactionDate(request.TransactionDate);
        var (dayStartUtc, dayEndUtcExclusive) = GetUtcDayRange(transactionDate);
        var hasDuplicate = await _accountRepository.HasDuplicateTransactionAsync(
            userId,
            request.AccountId,
            request.Amount,
            transactionType,
            request.CategoryId,
            dayStartUtc,
            dayEndUtcExclusive,
            cancellationToken);

        if (hasDuplicate && !request.IgnoreDuplicateWarning)
        {
            throw new DuplicateTransactionException("A similar transaction already exists for this account, amount, type, category, and date.");
        }

        var source = TransactionSources.ToEnum(request.Source);
        var resolvedRate = await _exchangeRateResolver.ResolveAsync("USD", "ARS", account.ExchangeRateType, cancellationToken);
        var convertedAmountUSD = normalizedCurrency == "USD"
            ? request.Amount
            : request.Amount / resolvedRate.SellPrice;
        var convertedAmountARS = normalizedCurrency == "ARS"
            ? request.Amount
            : request.Amount * resolvedRate.BuyPrice;

        var transaction = Transaction.Create(
            request.AccountId,
            request.Amount,
            normalizedCurrency,
            transactionType,
            request.Description,
            convertedAmountUSD,
            convertedAmountARS,
            resolvedRate.Id,
            request.CategoryId,
            transactionDate: transactionDate,
            source: source,
            assistantRawInput: request.AssistantRawInput
        );

        await _accountRepository.AddTransactionAsync(transaction);
        await _accountRepository.SaveChangesAsync();

        await LearnCategoryCorrectionAsync(userId, request, transactionType, cancellationToken);

        return transaction.Id;
    }

    public async Task<bool> UpdateTransactionAsync(
        Guid transactionId,
        Guid userId,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var transaction = await _accountRepository.GetTransactionByIdAsync(transactionId, userId, cancellationToken);
        if (transaction is null)
        {
            return false;
        }

        if (transaction.TransferGroupId.HasValue)
        {
            throw new ArgumentException("Transfers cannot be edited. Delete and recreate the transfer instead.", nameof(transactionId));
        }

        var account = await _accountRepository.GetForUpdateAsync(transaction.AccountId, userId, cancellationToken);
        if (account is null)
        {
            throw new ArgumentException("Account not found.", nameof(transaction.AccountId));
        }

        if (request.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("Category id cannot be empty.", nameof(request.CategoryId));
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                throw new ArgumentException("Category not found.", nameof(request.CategoryId));
            }

            if (!category.IsSystem && category.UserId != userId)
            {
                throw new ArgumentException("Category does not belong to the current user.", nameof(request.CategoryId));
            }

            if (category.Type != transaction.TransactionType)
            {
                throw new ArgumentException("Category type must match transaction type.", nameof(request.CategoryId));
            }
        }

        var resolvedRate = await _exchangeRateResolver.ResolveAsync("USD", "ARS", account.ExchangeRateType, cancellationToken);
        var (convertedAmountUsd, convertedAmountArs) = CalculateConvertedAmounts(
            request.Amount,
            request.Currency,
            resolvedRate.BuyPrice,
            resolvedRate.SellPrice);

        transaction.UpdateDetails(
            request.Amount,
            request.Currency,
            request.CategoryId,
            convertedAmountUsd,
            convertedAmountArs,
            resolvedRate.Id);

        await _accountRepository.SaveChangesAsync();

        return true;
    }

    public async Task<Guid> CreateTransferAsync(Guid userId, CreateTransferRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FromAccountId == request.ToAccountId)
        {
            throw new ArgumentException("Source and destination accounts must be different.", nameof(request.ToAccountId));
        }

        var fromAccount = await _accountRepository.GetForUpdateAsync(request.FromAccountId, userId, cancellationToken);
        if (fromAccount is null)
        {
            throw new ArgumentException("Source account not found.", nameof(request.FromAccountId));
        }

        var toAccount = await _accountRepository.GetForUpdateAsync(request.ToAccountId, userId, cancellationToken);
        if (toAccount is null)
        {
            throw new ArgumentException("Destination account not found.", nameof(request.ToAccountId));
        }

        var resolvedRate = await _exchangeRateResolver.ResolveAsync("USD", "ARS", fromAccount.ExchangeRateType, cancellationToken);

        var convertedAmountUsd = request.Currency == "USD"
            ? request.Amount
            : request.Amount / resolvedRate.SellPrice;
        var convertedAmountArs = request.Currency == "ARS"
            ? request.Amount
            : request.Amount * resolvedRate.BuyPrice;

        var transferGroupId = Guid.NewGuid();
        var baseDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description;

        var fromTransaction = Transaction.Create(
            request.FromAccountId,
            request.Amount,
            request.Currency,
            TransactionType.Expense,
            baseDescription ?? $"Transfer to {toAccount.Name}",
            convertedAmountUsd,
            convertedAmountArs,
            resolvedRate.Id,
            null,
            transferGroupId,
            request.ToAccountId);

        var toTransaction = Transaction.Create(
            request.ToAccountId,
            request.Amount,
            request.Currency,
            TransactionType.Income,
            baseDescription ?? $"Transfer from {fromAccount.Name}",
            convertedAmountUsd,
            convertedAmountArs,
            resolvedRate.Id,
            null,
            transferGroupId,
            request.FromAccountId);

        await _accountRepository.AddTransactionAsync(fromTransaction);
        await _accountRepository.AddTransactionAsync(toTransaction);
        await _accountRepository.SaveChangesAsync();

        return transferGroupId;
    }

    public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var transaction = await _accountRepository.GetTransactionByIdAsync(transactionId, userId, cancellationToken);

        if (transaction is null)
        {
            return false;
        }

        if (transaction.TransferGroupId.HasValue)
        {
            var groupedTransactions = await _accountRepository.GetTransactionsByTransferGroupIdAsync(
                transaction.TransferGroupId.Value,
                userId,
                cancellationToken);

            foreach (var groupedTransaction in groupedTransactions)
            {
                await _accountRepository.DeleteTransactionAsync(groupedTransaction, cancellationToken);
            }
        }
        else
        {
            await _accountRepository.DeleteTransactionAsync(transaction, cancellationToken);
        }

        await _accountRepository.SaveChangesAsync();

        return true;
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, userId, cancellationToken);

        return account is null ? null : Map(account);
    }

    public async Task<AccountDetailDto?> GetAccountDetailByIdAsync(
        Guid id,
        Guid userId,
        int transactionLimit = 50,
        CancellationToken cancellationToken = default)
    {
        var normalizedTransactionLimit = Math.Clamp(transactionLimit, 1, 200);
        var account = await _accountRepository.GetDetailByIdAsync(id, userId, normalizedTransactionLimit, cancellationToken);

        return account is null ? null : MapDetail(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetAllAsync(userId, cancellationToken);

        return accounts.Select(account => Map(account)).ToList();
    }

    public async Task<PagedResultDto<DashboardRecentTransactionDto>> GetRecentTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 50);

        return await _accountRepository.GetRecentTransactionsAsync(userId, normalizedPage, normalizedPageSize, cancellationToken);
    }

    public async Task<MonthlyTransactionSummaryDto> GetMonthlyTransactionSummaryAsync(
        Guid userId,
        int? month = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var normalizedMonth = month ?? now.Month;
        var normalizedYear = year ?? now.Year;

        if (normalizedMonth is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.", nameof(month));
        }

        if (normalizedYear < 1)
        {
            throw new ArgumentException("Year must be greater than zero.", nameof(year));
        }

        var fromUtc = new DateTime(normalizedYear, normalizedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtcExclusive = fromUtc.AddMonths(1);

        return await _accountRepository.GetMonthlyTransactionSummaryAsync(userId, fromUtc, toUtcExclusive, cancellationToken);
    }

    private static AccountDto Map(AccountBalanceSnapshot account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            ExchangeRateType = ExchangeRateTypes.ToString(account.ExchangeRateType),
            AccountType = AccountTypes.ToString(account.AccountType),
            FundingAccountId = account.FundingAccountId,
            PaymentDayOfMonth = account.PaymentDayOfMonth,
            BalanceInAccountCurrency = account.BalanceInAccountCurrency,
            BalanceUsd = account.BalanceUsd,
            BalanceArs = account.BalanceArs
        };
    }

    private static AccountDetailDto MapDetail(AccountDetailSnapshot account)
    {
        return new AccountDetailDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            ExchangeRateType = ExchangeRateTypes.ToString(account.ExchangeRateType),
            AccountType = AccountTypes.ToString(account.AccountType),
            FundingAccountId = account.FundingAccountId,
            PaymentDayOfMonth = account.PaymentDayOfMonth,
            BalanceInAccountCurrency = account.BalanceInAccountCurrency,
            BalanceUsd = account.BalanceUsd,
            BalanceArs = account.BalanceArs,
            Transactions = account.Transactions
        };
    }

    private async Task ValidateFundingAccountAsync(
        Guid userId,
        AccountType accountType,
        Guid? fundingAccountId,
        Guid? creditAccountId,
        CancellationToken cancellationToken)
    {
        if (accountType != AccountType.Credit)
        {
            return;
        }

        if (!fundingAccountId.HasValue)
        {
            return;
        }

        if (creditAccountId.HasValue && fundingAccountId.Value == creditAccountId.Value)
        {
            throw new ArgumentException("Funding account must be different from the credit account.", nameof(fundingAccountId));
        }

        var fundingAccount = await _accountRepository.GetForUpdateAsync(fundingAccountId.Value, userId, cancellationToken);
        if (fundingAccount is null)
        {
            throw new ArgumentException("Funding account not found.", nameof(fundingAccountId));
        }

        if (fundingAccount.AccountType == AccountType.Credit)
        {
            throw new ArgumentException("Funding account cannot be a credit account.", nameof(fundingAccountId));
        }
    }

    private static (decimal convertedAmountUsd, decimal convertedAmountArs) CalculateConvertedAmounts(
        decimal amount,
        string currency,
        decimal buyRate,
        decimal sellRate)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        return normalizedCurrency switch
        {
            "USD" => (amount, amount * buyRate),
            "ARS" => (amount / sellRate, amount),
            _ => throw new ArgumentException("Currency must be USD or ARS.", nameof(currency)),
        };
    }

    private async Task ValidateCategoryAsync(Guid userId, Guid? categoryId, TransactionType transactionType, CancellationToken cancellationToken)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));
        }

        if (!categoryId.HasValue)
        {
            return;
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken);
        if (category is null)
        {
            throw new ArgumentException("Category not found.", nameof(categoryId));
        }

        if (!category.IsSystem && category.UserId != userId)
        {
            throw new ArgumentException("Category does not belong to the current user.", nameof(categoryId));
        }

        if (category.Type != transactionType)
        {
            throw new ArgumentException("Category type must match transaction type.", nameof(categoryId));
        }
    }

    private async Task LearnCategoryCorrectionAsync(Guid userId, CreateTransactionRequest request, TransactionType transactionType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AssistantLearningKey) || !request.CategoryId.HasValue)
        {
            return;
        }

        if (request.AssistantSuggestedCategoryId == request.CategoryId)
        {
            return;
        }

        var normalizedKey = LearnedCategoryMapping.NormalizeKey(request.AssistantLearningKey);
        var mapping = await _learnedCategoryMappingRepository.GetAsync(userId, normalizedKey, transactionType, cancellationToken);
        if (mapping is null)
        {
            await _learnedCategoryMappingRepository.AddAsync(
                LearnedCategoryMapping.Create(userId, normalizedKey, transactionType, request.CategoryId.Value),
                cancellationToken);
        }
        else
        {
            mapping.UpdateCategory(request.CategoryId.Value);
        }

        await _learnedCategoryMappingRepository.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeAssistantCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return "ARS";
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        return normalizedCurrency switch
        {
            "ARS" or "USD" => normalizedCurrency,
            _ => throw new ArgumentException("Currency must be ARS or USD.", nameof(currency))
        };
    }

    private static DateTime NormalizeTransactionDate(DateTime? transactionDate)
    {
        var date = transactionDate?.Date ?? DateTime.UtcNow.Date;
        return DateTime.SpecifyKind(date.AddHours(12), DateTimeKind.Utc);
    }

    private static (DateTime fromUtc, DateTime toUtcExclusive) GetUtcDayRange(DateTime transactionDate)
    {
        var date = transactionDate.Date;
        return (DateTime.SpecifyKind(date, DateTimeKind.Utc), DateTime.SpecifyKind(date.AddDays(1), DateTimeKind.Utc));
    }
}
