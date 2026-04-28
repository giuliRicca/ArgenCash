using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
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

        public async Task<Account?> GetForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .SingleOrDefaultAsync(account => account.Id == id && account.UserId == userId, cancellationToken);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .SingleOrDefaultAsync(
                    transaction => transaction.Id == transactionId &&
                                   _context.Accounts.Any(account => account.Id == transaction.AccountId && account.UserId == userId),
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Transaction>> GetTransactionsByTransferGroupIdAsync(Guid transferGroupId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(transaction => transaction.TransferGroupId == transferGroupId &&
                                      _context.Accounts.Any(account => account.Id == transaction.AccountId && account.UserId == userId))
                .ToListAsync(cancellationToken);
        }

        public Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _context.Transactions.Remove(transaction);
            return Task.CompletedTask;
        }

        public async Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(account => account.Id == id && account.UserId == userId, cancellationToken);

            if (account is null)
            {
                return null;
            }

            var balancesByAccountId = await GetBalancesByAccountIdAsync([account.Id], cancellationToken);
            return MapAccountBalance(account, balancesByAccountId.GetValueOrDefault(account.Id));
        }

        public async Task<AccountDetailSnapshot?> GetDetailByIdAsync(
            Guid id,
            Guid userId,
            int transactionLimit = 50,
            CancellationToken cancellationToken = default)
        {
            var account = await GetByIdAsync(id, userId, cancellationToken);

            if (account is null)
            {
                return null;
            }

            var normalizedTransactionLimit = Math.Clamp(transactionLimit, 1, 200);
            var transactions = await (
                from transaction in _context.Transactions.AsNoTracking()
                join counterpartyAccount in _context.Accounts.AsNoTracking()
                    on transaction.CounterpartyAccountId equals (Guid?)counterpartyAccount.Id into counterpartyAccounts
                from counterpartyAccount in counterpartyAccounts.DefaultIfEmpty()
                join category in _context.Categories.AsNoTracking()
                    on transaction.CategoryId equals (Guid?)category.Id into categories
                from category in categories.DefaultIfEmpty()
                where transaction.AccountId == id
                orderby transaction.TransactionDate descending
                select new AccountTransactionDto
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    TransactionType = TransactionTypes.ToString(transaction.TransactionType),
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    ConvertedAmountUsd = transaction.ConvertedAmountUSD,
                    ConvertedAmountArs = transaction.ConvertedAmountARS,
                    TransactionDate = transaction.TransactionDate,
                    TransferGroupId = transaction.TransferGroupId,
                    CounterpartyAccountId = transaction.CounterpartyAccountId,
                    CounterpartyAccountName = counterpartyAccount == null ? null : counterpartyAccount.Name,
                    CategoryId = transaction.CategoryId,
                    CategoryName = category == null ? null : category.Name
                })
                .Take(normalizedTransactionLimit)
                .ToListAsync(cancellationToken);

            return new AccountDetailSnapshot
            {
                Id = account.Id,
                Name = account.Name,
                CurrencyCode = account.CurrencyCode,
                ExchangeRateType = account.ExchangeRateType,
                AccountType = account.AccountType,
                FundingAccountId = account.FundingAccountId,
                PaymentDayOfMonth = account.PaymentDayOfMonth,
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

        public async Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == userId)
                .OrderBy(account => account.Name)
                .ToListAsync(cancellationToken);

            if (accounts.Count == 0)
            {
                return [];
            }

            var accountIds = accounts.Select(account => account.Id).ToArray();
            var balancesByAccountId = await GetBalancesByAccountIdAsync(accountIds, cancellationToken);

            return accounts
                .Select(account => MapAccountBalance(account, balancesByAccountId.GetValueOrDefault(account.Id)))
                .ToList();
        }

        public async Task<PagedResultDto<DashboardRecentTransactionDto>> GetRecentTransactionsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var normalizedPage = Math.Max(page, 1);
            var normalizedPageSize = Math.Clamp(pageSize, 1, 50);
            var skip = (normalizedPage - 1) * normalizedPageSize;

            var query =
                from transaction in _context.Transactions.AsNoTracking()
                join account in _context.Accounts.AsNoTracking().Where(account => account.UserId == userId)
                    on transaction.AccountId equals account.Id
                join counterpartyAccount in _context.Accounts.AsNoTracking()
                    on transaction.CounterpartyAccountId equals (Guid?)counterpartyAccount.Id into counterpartyAccounts
                from counterpartyAccount in counterpartyAccounts.DefaultIfEmpty()
                join category in _context.Categories.AsNoTracking()
                    on transaction.CategoryId equals (Guid?)category.Id into categories
                from category in categories.DefaultIfEmpty()
                select new DashboardRecentTransactionDto
                {
                    Id = transaction.Id,
                    AccountId = transaction.AccountId,
                    AccountName = account.Name,
                    Amount = transaction.Amount,
                    TransactionType = TransactionTypes.ToString(transaction.TransactionType),
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    ConvertedAmountUsd = transaction.ConvertedAmountUSD,
                    ConvertedAmountArs = transaction.ConvertedAmountARS,
                    TransactionDate = transaction.TransactionDate,
                    TransferGroupId = transaction.TransferGroupId,
                    CounterpartyAccountId = transaction.CounterpartyAccountId,
                    CounterpartyAccountName = counterpartyAccount == null ? null : counterpartyAccount.Name,
                    CategoryId = transaction.CategoryId,
                    CategoryName = category == null ? null : category.Name
                };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(transaction => transaction.TransactionDate)
                .ThenByDescending(transaction => transaction.Id)
                .Skip(skip)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<DashboardRecentTransactionDto>
            {
                Items = items,
                Page = normalizedPage,
                PageSize = normalizedPageSize,
                TotalCount = totalCount,
                TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)normalizedPageSize)
            };
        }

        public async Task<MonthlyTransactionSummaryDto> GetMonthlyTransactionSummaryAsync(
            Guid userId,
            DateTime fromUtc,
            DateTime toUtcExclusive,
            CancellationToken cancellationToken = default)
        {
            var transactions = await (
                from transaction in _context.Transactions.AsNoTracking()
                join account in _context.Accounts.AsNoTracking().Where(account => account.UserId == userId)
                    on transaction.AccountId equals account.Id
                where transaction.TransactionDate >= fromUtc &&
                      transaction.TransactionDate < toUtcExclusive &&
                      transaction.TransferGroupId == null
                group transaction by transaction.TransactionType into groupedTransactions
                select new
                {
                    TransactionType = groupedTransactions.Key,
                    TotalUsd = groupedTransactions.Sum(transaction => transaction.ConvertedAmountUSD)
                })
                .ToListAsync(cancellationToken);

            var incomeUsd = transactions
                .Where(transaction => transaction.TransactionType == TransactionType.Income)
                .Sum(transaction => transaction.TotalUsd);
            var expenseUsd = transactions
                .Where(transaction => transaction.TransactionType == TransactionType.Expense)
                .Sum(transaction => transaction.TotalUsd);

            return new MonthlyTransactionSummaryDto
            {
                IncomeUsd = incomeUsd,
                ExpenseUsd = expenseUsd,
                NetUsd = incomeUsd - expenseUsd
            };
        }

        public async Task<List<CreditAccountSettlementCandidateSnapshot>> GetCreditSettlementCandidatesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .AsNoTracking()
                .Where(account =>
                    account.UserId == userId &&
                    account.AccountType == AccountType.Credit &&
                    account.FundingAccountId.HasValue &&
                    account.PaymentDayOfMonth.HasValue)
                .Select(account => new CreditAccountSettlementCandidateSnapshot
                {
                    Id = account.Id,
                    CurrencyCode = account.CurrencyCode,
                    ExchangeRateType = account.ExchangeRateType,
                    FundingAccountId = account.FundingAccountId!.Value,
                    PaymentDayOfMonth = account.PaymentDayOfMonth!.Value
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetCreditStatementNetExpenseAsync(
            Guid creditAccountId,
            DateTime fromUtc,
            DateTime toUtcExclusive,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.AccountId == creditAccountId &&
                    transaction.TransferGroupId == null &&
                    transaction.TransactionDate >= fromUtc &&
                    transaction.TransactionDate < toUtcExclusive)
                .Select(transaction => (decimal?)(transaction.TransactionType == TransactionType.Expense
                    ? transaction.Amount
                    : -transaction.Amount))
                .SumAsync(cancellationToken) ?? 0m;
        }

        private async Task<Dictionary<Guid, AccountBalanceTotals>> GetBalancesByAccountIdAsync(Guid[] accountIds, CancellationToken cancellationToken = default)
        {
            if (accountIds.Length == 0)
            {
                return [];
            }

            return await _context.Transactions
                .AsNoTracking()
                .Where(transaction => accountIds.Contains(transaction.AccountId))
                .GroupBy(transaction => transaction.AccountId)
                .Select(group => new AccountBalanceTotals
                {
                    AccountId = group.Key,
                    BalanceInAccountCurrency = group.Sum(transaction => transaction.TransactionType == TransactionType.Expense ? -transaction.Amount : transaction.Amount),
                    BalanceUsd = group.Sum(transaction => transaction.TransactionType == TransactionType.Expense ? -transaction.ConvertedAmountUSD : transaction.ConvertedAmountUSD),
                    BalanceArs = group.Sum(transaction => transaction.TransactionType == TransactionType.Expense ? -transaction.ConvertedAmountARS : transaction.ConvertedAmountARS)
                })
                .ToDictionaryAsync(balance => balance.AccountId, cancellationToken);
        }

        private static AccountBalanceSnapshot MapAccountBalance(Account account, AccountBalanceTotals? balances)
        {
            return new AccountBalanceSnapshot
            {
                Id = account.Id,
                Name = account.Name,
                CurrencyCode = account.CurrencyCode,
                ExchangeRateType = account.ExchangeRateType,
                AccountType = account.AccountType,
                FundingAccountId = account.FundingAccountId,
                PaymentDayOfMonth = account.PaymentDayOfMonth,
                BalanceInAccountCurrency = balances?.BalanceInAccountCurrency ?? 0m,
                BalanceUsd = balances?.BalanceUsd ?? 0m,
                BalanceArs = balances?.BalanceArs ?? 0m,
            };
        }

        private sealed class AccountBalanceTotals
        {
            public Guid AccountId { get; init; }
            public decimal BalanceInAccountCurrency { get; init; }
            public decimal BalanceUsd { get; init; }
            public decimal BalanceArs { get; init; }
        }

    }
}
