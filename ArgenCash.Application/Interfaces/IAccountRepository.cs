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
    Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId);
    Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId);
    Task<List<CreditAccountSettlementCandidateSnapshot>> GetCreditSettlementCandidatesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetCreditStatementNetExpenseAsync(Guid creditAccountId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken cancellationToken = default);
    Task SaveChangesAsync();
}
