using ArgenCash.Application.Interfaces;
using ArgenCash.Infrastructure.Authentication;
using ArgenCash.Infrastructure.ExchangeRates;
using ArgenCash.Infrastructure.Persistence;
using ArgenCash.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArgenCash.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer) &&
                !string.IsNullOrWhiteSpace(options.Audience) &&
                !string.IsNullOrWhiteSpace(options.SecretKey) &&
                options.SecretKey.Length >= 32 &&
                options.ExpirationMinutes > 0,
                "JWT settings are invalid.")
            .ValidateOnStart();

        services.AddOptions<ExchangeRateApiOptions>()
            .Bind(configuration.GetSection(ExchangeRateApiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Exchange-rate API settings are invalid.")
            .ValidateOnStart();

        services.AddDbContext<ArgenCashDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient<ILiveExchangeRateProvider, OpenExchangeRateProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ExchangeRateApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
