using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILiveExchangeRateProvider _exchangeRateProvider;

    public AccountService(IAccountRepository accountRepository, ILiveExchangeRateProvider exchangeRateProvider)
    {
        _accountRepository = accountRepository;
        _exchangeRateProvider = exchangeRateProvider;
    }

    public async Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = Account.Create(request.Name, request.CurrencyCode, userId);

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

        return account.Id;
    }

    public async Task<Guid> CreateTransactionAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = await _accountRepository.GetByIdAsync(request.AccountId, userId);
        if (account is null)
        {
            throw new ArgumentException("Account not found.", nameof(request.AccountId));
        }

        var liveRate = await _exchangeRateProvider.GetLiveRateAsync("USD", "ARS", cancellationToken);
        var convertedAmountUSD = request.Currency == "USD"
            ? request.Amount
            : request.Amount / liveRate.SellRate;
        var convertedAmountARS = request.Currency == "ARS"
            ? request.Amount
            : request.Amount * liveRate.BuyRate;

        var transactionType = TransactionTypes.ToEnum(request.TransactionType);

        var transaction = Transaction.Create(
            request.AccountId,
            request.Amount,
            request.Currency,
            transactionType,
            request.Description,
            convertedAmountUSD,
            convertedAmountARS,
            null,
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
            BalanceInAccountCurrency = account.BalanceInAccountCurrency,
            BalanceUsd = account.BalanceUsd,
            BalanceArs = account.BalanceArs,
            Transactions = account.Transactions
        };
    }
}
