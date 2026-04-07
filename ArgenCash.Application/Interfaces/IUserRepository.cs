using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
