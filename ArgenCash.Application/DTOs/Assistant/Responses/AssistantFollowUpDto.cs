namespace ArgenCash.Application.DTOs.Assistant.Responses;

public class AssistantFollowUpDto
{
    public string Field { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<AssistantFollowUpOptionDto> Options { get; set; } = [];
}
