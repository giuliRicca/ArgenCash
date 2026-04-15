using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
