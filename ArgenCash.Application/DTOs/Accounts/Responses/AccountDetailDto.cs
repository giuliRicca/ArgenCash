using ArgenCash.Application.DTOs.Transactions.Responses;

namespace ArgenCash.Application.DTOs.Accounts.Responses;

public class AccountDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string ExchangeRateType { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public Guid? FundingAccountId { get; init; }
    public int? PaymentDayOfMonth { get; init; }
    public decimal BalanceInAccountCurrency { get; init; }
    public decimal BalanceUsd { get; init; }
    public decimal BalanceArs { get; init; }
    public IReadOnlyList<AccountTransactionDto> Transactions { get; init; } = [];
}
