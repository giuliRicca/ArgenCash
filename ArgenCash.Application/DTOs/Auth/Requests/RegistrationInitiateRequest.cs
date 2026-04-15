namespace ArgenCash.Application.DTOs.Auth.Requests;

public class RegistrationInitiateRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
