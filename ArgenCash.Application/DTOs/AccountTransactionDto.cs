namespace ArgenCash.Application.DTOs;

public class AccountTransactionDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal ConvertedAmountUsd { get; init; }
    public decimal ConvertedAmountArs { get; init; }
    public DateTime TransactionDate { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
