namespace ArgenCash.Application.DTOs.Accounts.Responses;

public class AccountDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string ExchangeRateType { get; init; } = string.Empty;
    public decimal BalanceInAccountCurrency { get; init; }
    public decimal BalanceUsd { get; init; }
    public decimal BalanceArs { get; init; }
}
