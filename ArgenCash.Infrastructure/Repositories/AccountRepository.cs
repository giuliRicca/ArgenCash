using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using ArgenCash.Application.DTOs;
using ArgenCash.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArgenCash.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository

    {
        private readonly ArgenCashDbContext _context;

        public AccountRepository(ArgenCashDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public async Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId)
        {
            return await BuildAccountBalanceQuery(userId)
                .SingleOrDefaultAsync(account => account.Id == id);
        }

        public async Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId)
        {
            var account = await BuildAccountBalanceQuery(userId)
                .SingleOrDefaultAsync(account => account.Id == id);

            if (account is null)
            {
                return null;
            }

            var transactions = await _context.Transactions
                .AsNoTracking()
                .Where(transaction => transaction.AccountId == id)
                .OrderByDescending(transaction => transaction.TransactionDate)
                .Select(transaction => new AccountTransactionDto
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    TransactionType = transaction.TransactionType,
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    ConvertedAmountUsd = transaction.ConvertedAmountUSD,
                    ConvertedAmountArs = transaction.ConvertedAmountARS,
                    TransactionDate = transaction.TransactionDate
                })
                .ToListAsync();

            return new AccountDetailSnapshot
            {
                Id = account.Id,
                Name = account.Name,
                CurrencyCode = account.CurrencyCode,
                BalanceInAccountCurrency = account.BalanceInAccountCurrency,
                BalanceUsd = account.BalanceUsd,
                BalanceArs = account.BalanceArs,
                Transactions = transactions
            };
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId)
        {
            return await BuildAccountBalanceQuery(userId)
                .OrderBy(account => account.Name)
                .ToListAsync();
        }

        private IQueryable<AccountBalanceSnapshot> BuildAccountBalanceQuery(Guid userId)
        {
            return _context.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == userId)
                .Select(account => new AccountBalanceSnapshot
                {
                    Id = account.Id,
                    Name = account.Name,
                    CurrencyCode = account.CurrencyCode,
                    BalanceInAccountCurrency = _context.Transactions
                        .Where(transaction => transaction.AccountId == account.Id)
                        .Select(transaction => (decimal?)(transaction.TransactionType == TransactionTypes.Expense ? -transaction.Amount : transaction.Amount))
                        .Sum() ?? 0m,
                    BalanceUsd = _context.Transactions
                        .Where(transaction => transaction.AccountId == account.Id)
                        .Select(transaction => (decimal?)(transaction.TransactionType == TransactionTypes.Expense ? -transaction.ConvertedAmountUSD : transaction.ConvertedAmountUSD))
                        .Sum() ?? 0m,
                    BalanceArs = _context.Transactions
                        .Where(transaction => transaction.AccountId == account.Id)
                        .Select(transaction => (decimal?)(transaction.TransactionType == TransactionTypes.Expense ? -transaction.ConvertedAmountARS : transaction.ConvertedAmountARS))
                        .Sum() ?? 0m,
                });
        }
    }
}
