namespace ArgenCash.Application.DTOs
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string ExchangeRateType { get; set; } = string.Empty;
        public decimal BalanceInAccountCurrency { get; set; }
        public decimal BalanceUsd { get; set; }
        public decimal BalanceArs { get; set; }
    }
}
