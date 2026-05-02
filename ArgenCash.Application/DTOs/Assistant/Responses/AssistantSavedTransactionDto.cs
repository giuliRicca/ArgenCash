namespace ArgenCash.Application.DTOs.Assistant.Responses;

public class AssistantSavedTransactionDto
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Source { get; set; } = string.Empty;
}
