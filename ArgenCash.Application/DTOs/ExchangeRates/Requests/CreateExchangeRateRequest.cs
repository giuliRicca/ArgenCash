namespace ArgenCash.Application.DTOs.ExchangeRates.Requests
{
    public class CreateExchangeRateRequest
    {
        public string RateType { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = "USD";
        public string TargetCurrency { get; set; } = "ARS";
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
