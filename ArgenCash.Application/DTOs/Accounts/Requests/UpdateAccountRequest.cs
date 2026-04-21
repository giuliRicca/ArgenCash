using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs.Accounts.Requests;

public class UpdateAccountRequest
{
    public string? Name { get; set; }
    public ExchangeRateType? ExchangeRateType { get; set; }
    public AccountType? AccountType { get; set; }
    public Guid? FundingAccountId { get; set; }
    public int? PaymentDayOfMonth { get; set; }
}
