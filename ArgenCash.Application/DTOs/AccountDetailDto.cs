namespace ArgenCash.Application.DTOs;

public class AccountDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal BalanceInAccountCurrency { get; init; }
    public decimal BalanceUsd { get; init; }
    public decimal BalanceArs { get; init; }
    public IReadOnlyList<AccountTransactionDto> Transactions { get; init; } = [];
}
