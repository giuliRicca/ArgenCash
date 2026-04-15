namespace ArgenCash.Infrastructure.Authentication;

public class VerificationTokenOptions
{
    public const string SectionName = "VerificationToken";

    public string SecretKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 15;
    public string Issuer { get; init; } = "ArgenCash.Api";
}