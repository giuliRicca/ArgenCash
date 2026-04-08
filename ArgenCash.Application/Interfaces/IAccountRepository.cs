using ArgenCash.Application.DTOs;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account);
    Task AddTransactionAsync(Transaction transaction);
    Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId);
    Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId);
    Task SaveChangesAsync();
}
