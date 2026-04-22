namespace ArgenCash.Application.DTOs.Transactions.Responses;

public class DashboardRecentTransactionDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal ConvertedAmountUsd { get; init; }
    public decimal ConvertedAmountArs { get; init; }
    public DateTime TransactionDate { get; init; }
    public Guid? TransferGroupId { get; init; }
    public Guid? CounterpartyAccountId { get; init; }
    public string? CounterpartyAccountName { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
