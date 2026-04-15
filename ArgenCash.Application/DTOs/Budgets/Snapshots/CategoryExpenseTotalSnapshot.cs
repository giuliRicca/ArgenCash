namespace ArgenCash.Application.DTOs.Budgets.Snapshots;

public class CategoryExpenseTotalSnapshot
{
    public Guid CategoryId { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal TotalArs { get; set; }
}
