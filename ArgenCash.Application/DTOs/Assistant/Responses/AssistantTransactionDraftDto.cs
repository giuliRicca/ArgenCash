namespace ArgenCash.Application.DTOs.Assistant.Responses;

public class AssistantTransactionDraftDto
{
    public Guid? AccountId { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "ARS";
    public bool WasCurrencyDefaulted { get; set; }
    public string? TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public bool CategorySkipped { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? LearningKey { get; set; }
    public Guid? SuggestedCategoryId { get; set; }
}
