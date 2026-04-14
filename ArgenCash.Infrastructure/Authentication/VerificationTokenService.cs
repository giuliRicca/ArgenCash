using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArgenCash.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ArgenCash.Infrastructure.Authentication;

public class VerificationTokenService : IVerificationTokenService
{
    private readonly VerificationTokenOptions _options;

    public VerificationTokenService(IOptions<VerificationTokenOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(RegistrationInitiateRequest request)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, request.FullName),
            new Claim(JwtRegisteredClaimNames.Email, request.Email.ToLowerInvariant()),
            new Claim("pwd", request.Password),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Issuer,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RegistrationInitiateRequest? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return null;
            }

            var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var password = jwtToken.Claims.FirstOrDefault(c => c.Type == "pwd")?.Value;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            return new RegistrationInitiateRequest
            {
                FullName = fullName,
                Email = email,
                Password = password
            };
        }
        catch
        {
            return null;
        }
    }
}
