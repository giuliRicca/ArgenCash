namespace ArgenCash.Application.DTOs.ExchangeRates.Responses;

public class LiveExchangeRateDto
{
    public string BaseCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = string.Empty;
    public decimal BuyRate { get; init; }
    public decimal SellRate { get; init; }
    public DateTime RetrievedAtUtc { get; init; }
    public string Source { get; init; } = string.Empty;
}
