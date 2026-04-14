namespace ArgenCash.Application.DTOs.Auth.Requests;

public class VerifyEmailRequest
{
    public string VerificationToken { get; init; } = string.Empty;
}
