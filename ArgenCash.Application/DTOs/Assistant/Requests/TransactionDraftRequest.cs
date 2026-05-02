using ArgenCash.Application.DTOs.Assistant.Responses;

namespace ArgenCash.Application.DTOs.Assistant.Requests;

public class TransactionDraftRequest
{
    public string Text { get; set; } = string.Empty;
    public AssistantTransactionDraftDto? PreviousDraft { get; set; }
}
