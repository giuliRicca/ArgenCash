using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IExchangeRateResolver _exchangeRateResolver;

    public AccountService(IAccountRepository accountRepository, IExchangeRateResolver exchangeRateResolver)
    {
        _accountRepository = accountRepository;
        _exchangeRateResolver = exchangeRateResolver;
    }

    public async Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = Account.Create(request.Name, request.CurrencyCode, userId);

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

        if (!hasNameUpdate && !hasExchangeRateTypeUpdate)
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

        var resolvedRate = await _exchangeRateResolver.ResolveAsync("USD", "ARS", account.ExchangeRateType, cancellationToken);
        var convertedAmountUSD = request.Currency == "USD"
            ? request.Amount
            : request.Amount / resolvedRate.SellPrice;
        var convertedAmountARS = request.Currency == "ARS"
            ? request.Amount
            : request.Amount * resolvedRate.BuyPrice;

        var transactionType = TransactionTypes.ToEnum(request.TransactionType);

        var transaction = Transaction.Create(
            request.AccountId,
            request.Amount,
            request.Currency,
            transactionType,
            request.Description,
            convertedAmountUSD,
            convertedAmountARS,
            resolvedRate.Id,
            request.CategoryId
        );

        await _accountRepository.AddTransactionAsync(transaction);
        await _accountRepository.SaveChangesAsync();

        return transaction.Id;
    }

    public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var transaction = await _accountRepository.GetTransactionByIdAsync(transactionId, userId, cancellationToken);

        if (transaction is null)
        {
            return false;
        }

        await _accountRepository.DeleteTransactionAsync(transaction, cancellationToken);
        await _accountRepository.SaveChangesAsync();

        return true;
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, Guid userId)
    {
        var account = await _accountRepository.GetByIdAsync(id, userId);

        return account is null ? null : Map(account);
    }

    public async Task<AccountDetailDto?> GetAccountDetailByIdAsync(Guid id, Guid userId)
    {
        var account = await _accountRepository.GetDetailByIdAsync(id, userId);

        return account is null ? null : MapDetail(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId)
    {
        var accounts = await _accountRepository.GetAllAsync(userId);

        return accounts.Select(account => Map(account)).ToList();
    }

    private static AccountDto Map(AccountBalanceSnapshot account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            CurrencyCode = account.CurrencyCode,
            ExchangeRateType = ExchangeRateTypes.ToString(account.ExchangeRateType),
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
            BalanceInAccountCurrency = account.BalanceInAccountCurrency,
            BalanceUsd = account.BalanceUsd,
            BalanceArs = account.BalanceArs,
            Transactions = account.Transactions
        };
    }
}
