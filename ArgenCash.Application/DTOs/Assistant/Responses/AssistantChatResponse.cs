namespace ArgenCash.Application.DTOs.Assistant.Responses;

public class AssistantChatResponse
{
    public string Type { get; set; } = AssistantChatResponseTypes.Unsupported;
    public string Message { get; set; } = string.Empty;
    public AssistantSavedTransactionDto? Transaction { get; set; }
    public AssistantTransactionDraftDto? Draft { get; set; }
    public AssistantTransactionPreviewDto? TransactionPreview { get; set; }
    public AssistantFollowUpDto? FollowUp { get; set; }
    public List<string> Warnings { get; set; } = [];
}
