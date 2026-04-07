using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = Account.Create(request.Name, request.CurrencyCode, userId);

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

        return account.Id;
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
