using ArgenCash.Application.DTOs;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IExchangeRateService
{
    Task<Guid> CreateManualRateAsync(CreateExchangeRateRequest request);
    Task<ExchangeRateDto?> GetByIdAsync(Guid id);
    Task<ExchangeRateDto?> GetLatestAsync(string baseCurrency, string targetCurrency);
    Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType = ExchangeRateType.Official, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiveExchangeRateByTypeDto>> GetLiveRatesAsync(string baseCurrency, string targetCurrency, IReadOnlyCollection<ExchangeRateType> rateTypes, CancellationToken cancellationToken = default);
}
