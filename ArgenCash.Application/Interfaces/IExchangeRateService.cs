using ArgenCash.Application.DTOs;

namespace ArgenCash.Application.Interfaces;

public interface IExchangeRateService
{
    Task<Guid> CreateManualRateAsync(CreateExchangeRateRequest request);
    Task<ExchangeRateDto?> GetByIdAsync(Guid id);
    Task<ExchangeRateDto?> GetLatestAsync(string baseCurrency, string targetCurrency);
    Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default);
}
