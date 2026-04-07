using ArgenCash.Application.DTOs;

namespace ArgenCash.Application.Interfaces;

public interface ILiveExchangeRateProvider
{
    Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default);
}
