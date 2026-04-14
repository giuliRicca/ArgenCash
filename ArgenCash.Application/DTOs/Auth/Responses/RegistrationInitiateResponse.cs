namespace ArgenCash.Application.DTOs.Auth.Responses;

public class RegistrationInitiateResponse
{
    public string Message { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
