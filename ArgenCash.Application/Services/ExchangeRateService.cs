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

    public async Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType = ExchangeRateType.Official, CancellationToken cancellationToken = default)
    {
        return await _liveExchangeRateProvider.GetLiveRateAsync(
            baseCurrency,
            targetCurrency,
            rateType,
            cancellationToken);
    }

    public async Task<IReadOnlyList<LiveExchangeRateByTypeDto>> GetLiveRatesAsync(string baseCurrency, string targetCurrency, IReadOnlyCollection<ExchangeRateType> rateTypes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rateTypes);

        if (rateTypes.Count == 0)
        {
            throw new ArgumentException("At least one rate type must be requested.", nameof(rateTypes));
        }

        return await _liveExchangeRateProvider.GetLiveRatesAsync(baseCurrency, targetCurrency, rateTypes, cancellationToken);
    }

    private static ExchangeRateDto? Map(ExchangeRate? exchangeRate)
    {

        return exchangeRate is null
            ? null
            : new ExchangeRateDto
            {
                Id = exchangeRate.Id,
                RateType = ExchangeRateTypes.ToString(exchangeRate.RateType),
                BaseCurrency = exchangeRate.BaseCurrency,
                TargetCurrency = exchangeRate.TargetCurrency,
                BuyPrice = exchangeRate.BuyPrice,
                SellPrice = exchangeRate.SellPrice,
                EffectiveDate = exchangeRate.EffectiveDate
            };
    }
}
