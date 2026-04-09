using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IExchangeRateResolver
{
    Task<ExchangeRate> ResolveAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType, CancellationToken cancellationToken = default);
}
