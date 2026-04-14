namespace ArgenCash.Application.DTOs.Auth.Responses;

public class AuthenticatedUserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
