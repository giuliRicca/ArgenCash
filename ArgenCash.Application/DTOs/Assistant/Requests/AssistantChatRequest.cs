namespace ArgenCash.Application.DTOs.Assistant.Requests;

public class AssistantChatRequest
{
    public string Text { get; set; } = string.Empty;
    public string? Action { get; set; }
    public AssistantTransactionDraftDto? PreviousDraft { get; set; }
}
