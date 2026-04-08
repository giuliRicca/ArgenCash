using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ArgenCash.Infrastructure.ExchangeRates;

public class DolarApiProvider : ILiveExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ExchangeRateApiOptions _options;

    public DolarApiProvider(HttpClient httpClient, IOptions<ExchangeRateApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
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

        var response = await _httpClient.GetFromJsonAsync<List<DolarApiResponse>>("v1/dolares", cancellationToken)
            ?? throw new InvalidOperationException("Exchange-rate provider returned an empty response.");

        var officialRate = response.FirstOrDefault(r => string.Equals(r.Casa, "oficial", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Official exchange rate not found in provider response.");

        return new LiveExchangeRateDto
        {
            BaseCurrency = normalizedBaseCurrency,
            TargetCurrency = normalizedTargetCurrency,
            BuyRate = officialRate.Compra,
            SellRate = officialRate.Venta,
            RetrievedAtUtc = DateTime.UtcNow,
            Source = _options.SourceName
        };
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
