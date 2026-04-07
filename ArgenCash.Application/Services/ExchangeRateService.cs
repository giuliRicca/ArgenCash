using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly ILiveExchangeRateProvider _liveExchangeRateProvider;

    public ExchangeRateService(IExchangeRateRepository repository, ILiveExchangeRateProvider liveExchangeRateProvider)
    {
        _repository = repository;
        _liveExchangeRateProvider = liveExchangeRateProvider;
    }

    public async Task<Guid> CreateManualRateAsync(CreateExchangeRateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var effectiveDate = request.EffectiveDate ?? DateTime.UtcNow;

        var rate = ExchangeRate.Create(
            request.RateType,
            request.BaseCurrency,
            request.TargetCurrency,
            request.BuyPrice,
            request.SellPrice,
            effectiveDate
        );

        await _repository.AddAsync(rate);
        await _repository.SaveChangesAsync();

        return rate.Id;
    }

    public async Task<ExchangeRateDto?> GetByIdAsync(Guid id)
    {
        var exchangeRate = await _repository.GetByIdAsync(id);

        return Map(exchangeRate);
    }

    public async Task<ExchangeRateDto?> GetLatestAsync(string baseCurrency, string targetCurrency)
    {
        var exchangeRate = await _repository.GetLatestAsync(baseCurrency, targetCurrency);

        return Map(exchangeRate);
    }

    public async Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        return await _liveExchangeRateProvider.GetLiveRateAsync(baseCurrency, targetCurrency, cancellationToken);
    }

    private static ExchangeRateDto? Map(ExchangeRate? exchangeRate)
    {

        return exchangeRate is null
            ? null
            : new ExchangeRateDto
            {
                Id = exchangeRate.Id,
                RateType = exchangeRate.RateType,
                BaseCurrency = exchangeRate.BaseCurrency,
                TargetCurrency = exchangeRate.TargetCurrency,
                BuyPrice = exchangeRate.BuyPrice,
                SellPrice = exchangeRate.SellPrice,
                EffectiveDate = exchangeRate.EffectiveDate
            };
    }
}
