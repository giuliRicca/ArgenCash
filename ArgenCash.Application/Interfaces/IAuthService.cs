
namespace ArgenCash.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponseDto> LoginAsync(LoginRequest request);
    Task<AuthenticatedUserDto?> GetCurrentUserAsync(Guid userId);
    Task<bool> CheckUserExistsAsync(string email);
}
