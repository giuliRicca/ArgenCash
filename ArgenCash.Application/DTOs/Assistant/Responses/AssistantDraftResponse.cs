namespace ArgenCash.Application.DTOs.Assistant.Responses;

public class AssistantDraftResponse
{
    public string State { get; set; } = "unsupported";
    public AssistantTransactionDraftDto? Draft { get; set; }
    public AssistantFollowUpDto? FollowUp { get; set; }
    public List<string> Warnings { get; set; } = [];
    public string? Message { get; set; }
}
