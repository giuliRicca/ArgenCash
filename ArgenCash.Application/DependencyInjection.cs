using ArgenCash.Application.Interfaces;
using ArgenCash.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArgenCash.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<IExchangeRateResolver, ExchangeRateResolver>();
        services.AddScoped<IExchangeRateUsabilityPolicy, ExchangeRateUsabilityPolicy>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
