using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using ArgenCash.Application.DTOs;
using ArgenCash.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ArgenCash.Infrastructure.ExchangeRates;

public class OpenExchangeRateProvider : ILiveExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ExchangeRateApiOptions _options;

    public OpenExchangeRateProvider(HttpClient httpClient, IOptions<ExchangeRateApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<LiveExchangeRateDto> GetLiveRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        var normalizedBaseCurrency = NormalizeCurrency(baseCurrency, nameof(baseCurrency));
        var normalizedTargetCurrency = NormalizeCurrency(targetCurrency, nameof(targetCurrency));

        var response = await _httpClient.GetFromJsonAsync<OpenExchangeRateResponse>($"v6/latest/{normalizedBaseCurrency}", cancellationToken)
            ?? throw new InvalidOperationException("Exchange-rate provider returned an empty response.");

        if (!string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Exchange-rate provider request was not successful.");
        }

        if (response.Rates is null || !response.Rates.TryGetValue(normalizedTargetCurrency, out var rate))
        {
            throw new InvalidOperationException($"Target currency '{normalizedTargetCurrency}' was not found in the provider response.");
        }

        return new LiveExchangeRateDto
        {
            BaseCurrency = normalizedBaseCurrency,
            TargetCurrency = normalizedTargetCurrency,
            Rate = rate,
            RetrievedAtUtc = ResolveRetrievedAtUtc(response),
            Source = _options.SourceName
        };
    }

    private static DateTime ResolveRetrievedAtUtc(OpenExchangeRateResponse response)
    {
        if (response.TimeLastUpdateUnix is > 0)
        {
            return DateTimeOffset.FromUnixTimeSeconds(response.TimeLastUpdateUnix.Value).UtcDateTime;
        }

        if (!string.IsNullOrWhiteSpace(response.TimeLastUpdateUtc)
            && DateTimeOffset.TryParse(
                response.TimeLastUpdateUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedTimestamp))
        {
            return parsedTimestamp.UtcDateTime;
        }

        return DateTime.UtcNow;
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

    private sealed class OpenExchangeRateResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = string.Empty;

        [JsonPropertyName("time_last_update_utc")]
        public string? TimeLastUpdateUtc { get; init; }

        [JsonPropertyName("time_last_update_unix")]
        public long? TimeLastUpdateUnix { get; init; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; init; }
    }
}
