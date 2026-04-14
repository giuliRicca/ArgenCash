namespace ArgenCash.Application.DTOs.Budgets.Requests;

public class UpdateBudgetRequest
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
