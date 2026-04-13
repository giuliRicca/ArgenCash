namespace ArgenCash.Application.DTOs;

public class CategoryExpenseTotalSnapshot
{
    public Guid CategoryId { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal TotalArs { get; set; }
}
