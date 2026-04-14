namespace ArgenCash.Application.DTOs.Transactions.Requests;

public class UpdateTransactionRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
}
