using ArgenCash.Application.DTOs.Transactions.Responses;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs.Accounts.Snapshots;

public class AccountDetailSnapshot
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public ExchangeRateType ExchangeRateType { get; init; }
    public AccountType AccountType { get; init; }
    public Guid? FundingAccountId { get; init; }
    public int? PaymentDayOfMonth { get; init; }
    public decimal BalanceInAccountCurrency { get; init; }
    public decimal BalanceUsd { get; init; }
    public decimal BalanceArs { get; init; }
    public IReadOnlyList<AccountTransactionDto> Transactions { get; init; } = [];
}
