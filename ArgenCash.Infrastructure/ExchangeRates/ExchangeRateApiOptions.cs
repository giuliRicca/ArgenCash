namespace ArgenCash.Infrastructure.ExchangeRates;

public class ExchangeRateApiOptions
{
    public const string SectionName = "ExchangeRateApi";

    public string BaseUrl { get; init; } = "https://dolarapi.com/";
    public string SourceName { get; init; } = "dolarapi.com";
}
