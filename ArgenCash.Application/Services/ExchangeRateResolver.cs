using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class ExchangeRateResolver : IExchangeRateResolver
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ILiveExchangeRateProvider _liveExchangeRateProvider;
    private readonly IExchangeRateUsabilityPolicy _usabilityPolicy;

    public ExchangeRateResolver(
        IExchangeRateRepository exchangeRateRepository,
        ILiveExchangeRateProvider liveExchangeRateProvider,
        IExchangeRateUsabilityPolicy usabilityPolicy)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _liveExchangeRateProvider = liveExchangeRateProvider;
        _usabilityPolicy = usabilityPolicy;
    }

    public async Task<ExchangeRate> ResolveAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType, CancellationToken cancellationToken = default)
    {
        var cachedRate = await _exchangeRateRepository.GetLatestAsync(baseCurrency, targetCurrency, rateType);
        if (cachedRate is not null && _usabilityPolicy.IsUsable(cachedRate, DateTime.UtcNow))
        {
            return cachedRate;
        }

        var liveRate = await _liveExchangeRateProvider.GetLiveRateAsync(baseCurrency, targetCurrency, rateType, cancellationToken);

        var persistedRate = ExchangeRate.Create(
            rateType,
            liveRate.BaseCurrency,
            liveRate.TargetCurrency,
            liveRate.BuyRate,
            liveRate.SellRate,
            liveRate.RetrievedAtUtc);

        await _exchangeRateRepository.AddAsync(persistedRate);
        await _exchangeRateRepository.SaveChangesAsync();

        return persistedRate;
    }
}
