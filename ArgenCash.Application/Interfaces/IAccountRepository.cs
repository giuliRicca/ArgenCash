using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account);
    Task<Account?> GetForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task AddTransactionAsync(Transaction transaction);
    Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetTransactionsByTransferGroupIdAsync(Guid transferGroupId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId, int transactionLimit = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardRecentTransactionDto>> GetRecentTransactionsAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<CreditAccountSettlementCandidateSnapshot>> GetCreditSettlementCandidatesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetCreditStatementNetExpenseAsync(Guid creditAccountId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default);
    Task SaveChangesAsync();
}
