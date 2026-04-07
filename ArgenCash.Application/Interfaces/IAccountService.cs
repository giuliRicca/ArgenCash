using ArgenCash.Application.DTOs;

namespace ArgenCash.Application.Interfaces;

public interface IAccountService
{
    Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request);
    Task<AccountDto?> GetAccountByIdAsync(Guid id, Guid userId);
    Task<AccountDetailDto?> GetAccountDetailByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId);
}
