using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ArgenCash.Infrastructure.ExchangeRates;

public class DolarApiProvider : ILiveExchangeRateProvider
{
    private static readonly TimeSpan RatesCacheDuration = TimeSpan.FromSeconds(45);

    private readonly HttpClient _httpClient;
    private readonly ExchangeRateApiOptions _options;
    private readonly IMemoryCache _memoryCache;

    public DolarApiProvider(HttpClient httpClient, IOptions<ExchangeRateApiOptions> options, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _memoryCache = memoryCache;
    }

    public async Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, ExchangeRateType rateType, CancellationToken cancellationToken = default)
    {
        var rates = await GetLiveRatesAsync(baseCurrency, targetCurrency, [rateType], cancellationToken);
        return rates[0].Rate;
    }

    public async Task<IReadOnlyList<LiveExchangeRateByTypeDto>> GetLiveRatesAsync(string baseCurrency, string targetCurrency, IReadOnlyCollection<ExchangeRateType> rateTypes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rateTypes);

        if (rateTypes.Count == 0)
        {
            throw new ArgumentException("At least one exchange rate type must be requested.", nameof(rateTypes));
        }

        var normalizedBaseCurrency = NormalizeCurrency(baseCurrency, nameof(baseCurrency));
        var normalizedTargetCurrency = NormalizeCurrency(targetCurrency, nameof(targetCurrency));

        if (!string.Equals(normalizedBaseCurrency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Only USD as base currency is supported. Received: {normalizedBaseCurrency}");
        }

        if (!string.Equals(normalizedTargetCurrency, "ARS", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Only ARS as target currency is supported. Received: {normalizedTargetCurrency}");
        }

        var response = await GetProviderRatesAsync(normalizedBaseCurrency, normalizedTargetCurrency, cancellationToken);
        var requestedRateTypes = rateTypes.Distinct().ToList();
        var retrievedAtUtc = DateTime.UtcNow;

        var results = new List<LiveExchangeRateByTypeDto>(requestedRateTypes.Count);

        foreach (var requestedRateType in requestedRateTypes)
        {
            var selectedRate = SelectRateByType(response, requestedRateType)
                ?? throw new InvalidOperationException($"Exchange rate type '{ExchangeRateTypes.ToString(requestedRateType)}' not found in provider response.");

            results.Add(new LiveExchangeRateByTypeDto
            {
                RateType = requestedRateType,
                Rate = new LiveExchangeRateDto
                {
                    BaseCurrency = normalizedBaseCurrency,
                    TargetCurrency = normalizedTargetCurrency,
                    BuyRate = selectedRate.Compra,
                    SellRate = selectedRate.Venta,
                    RetrievedAtUtc = retrievedAtUtc,
                    Source = _options.SourceName
                }
            });
        }

        return results;
    }

    private async Task<IReadOnlyList<DolarApiResponse>> GetProviderRatesAsync(string normalizedBaseCurrency, string normalizedTargetCurrency, CancellationToken cancellationToken)
    {
        var cacheKey = $"dolarapi:{normalizedBaseCurrency}:{normalizedTargetCurrency}";

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<DolarApiResponse>? cachedRates) && cachedRates is not null)
        {
            return cachedRates;
        }

        var response = await _httpClient.GetFromJsonAsync<List<DolarApiResponse>>("v1/dolares", cancellationToken)
            ?? throw new InvalidOperationException("Exchange-rate provider returned an empty response.");

        _memoryCache.Set(cacheKey, response, RatesCacheDuration);
        return response;
    }

    private static DolarApiResponse? SelectRateByType(IEnumerable<DolarApiResponse> rates, ExchangeRateType rateType)
    {
        return rateType switch
        {
            ExchangeRateType.Official => rates.FirstOrDefault(rate => EqualsAny(rate.Casa, "oficial")),
            ExchangeRateType.Ccl => rates.FirstOrDefault(rate => EqualsAny(rate.Casa, "contadoconliqui", "ccl") || ContainsAny(rate.Nombre, "contado con liqui", "ccl")),
            ExchangeRateType.Mep => rates.FirstOrDefault(rate => EqualsAny(rate.Casa, "bolsa", "mep") || ContainsAny(rate.Nombre, "bolsa", "mep")),
            ExchangeRateType.Blue => rates.FirstOrDefault(rate => EqualsAny(rate.Casa, "blue") || ContainsAny(rate.Nombre, "blue")),
            ExchangeRateType.Crypto => rates.FirstOrDefault(rate => EqualsAny(rate.Casa, "cripto", "crypto") || ContainsAny(rate.Nombre, "cripto", "crypto")),
            _ => null
        };
    }

    private static bool EqualsAny(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return candidates.Any(candidate => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsAny(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeCurrency(string currency, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency code is required.", parameterName);
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3 || !normalizedCurrency.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", parameterName);
        }

        return normalizedCurrency;
    }

    private sealed class DolarApiResponse
    {
        [JsonPropertyName("casa")]
        public string Casa { get; init; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; init; } = string.Empty;

        [JsonPropertyName("compra")]
        public decimal Compra { get; init; }

        [JsonPropertyName("venta")]
        public decimal Venta { get; init; }

        [JsonPropertyName("fecha")]
        public string? Fecha { get; init; }
    }
}
