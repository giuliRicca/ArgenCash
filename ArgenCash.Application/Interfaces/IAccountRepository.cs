using ArgenCash.Application.DTOs;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account);
    Task<AccountBalanceSnapshot?> GetByIdAsync(Guid id, Guid userId);
    Task<AccountDetailSnapshot?> GetDetailByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<AccountBalanceSnapshot>> GetAllAsync(Guid userId);
    Task SaveChangesAsync();
}
