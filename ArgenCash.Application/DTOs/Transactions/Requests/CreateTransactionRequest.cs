namespace ArgenCash.Application.DTOs.Transactions.Requests;

public class CreateTransactionRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? Source { get; set; }
    public bool IgnoreDuplicateWarning { get; set; }
    public string? AssistantLearningKey { get; set; }
    public Guid? AssistantSuggestedCategoryId { get; set; }
    public string? AssistantRawInput { get; set; }
}
