namespace ArgenCash.Application.DTOs;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public string RateType { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
}
