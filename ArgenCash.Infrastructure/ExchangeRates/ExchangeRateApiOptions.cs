namespace ArgenCash.Infrastructure.ExchangeRates;

public class ExchangeRateApiOptions
{
    public const string SectionName = "ExchangeRateApi";

    public string BaseUrl { get; init; } = "https://open.er-api.com/";
    public string SourceName { get; init; } = "open.er-api.com";
}
