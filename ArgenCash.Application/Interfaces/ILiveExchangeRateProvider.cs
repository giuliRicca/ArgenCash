using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface ILiveExchangeRateProvider
{
    Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiveExchangeRateByTypeDto>> GetLiveRatesAsync(string baseCurrency, string targetCurrency, IReadOnlyCollection<ExchangeRateType> rateTypes, CancellationToken cancellationToken = default);
}
