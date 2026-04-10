using ArgenCash.Application.DTOs;

namespace ArgenCash.Application.Interfaces;

public interface IAccountService
{
    Task<Guid> CreateAccountAsync(Guid userId, CreateAccountRequest request);
    Task<bool> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken = default);
    Task<Guid> CreateTransactionAsync(Guid userId, CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<Guid> CreateTransferAsync(Guid userId, CreateTransferRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetAccountByIdAsync(Guid id, Guid userId);
    Task<AccountDetailDto?> GetAccountDetailByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId);
}
