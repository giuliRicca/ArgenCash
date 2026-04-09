using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs;

public class AccountBalanceSnapshot
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public ExchangeRateType ExchangeRateType { get; init; }
    public decimal BalanceInAccountCurrency { get; init; }
    public decimal BalanceUsd { get; init; }
    public decimal BalanceArs { get; init; }
}
