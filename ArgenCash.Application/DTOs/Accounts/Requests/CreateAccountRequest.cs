using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs.Accounts.Requests
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public AccountType AccountType { get; set; } = AccountType.Standard;
        public Guid? FundingAccountId { get; set; }
        public int? PaymentDayOfMonth { get; set; }
    }
}
