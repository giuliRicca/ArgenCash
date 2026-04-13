namespace ArgenCash.Application.DTOs;

public class CreateBudgetRequest
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
