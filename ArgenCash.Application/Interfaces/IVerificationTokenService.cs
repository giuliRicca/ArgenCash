
namespace ArgenCash.Application.Interfaces;

public interface IVerificationTokenService
{
    string GenerateToken(RegistrationInitiateRequest request);
    RegistrationInitiateRequest? ValidateToken(string token);
}
