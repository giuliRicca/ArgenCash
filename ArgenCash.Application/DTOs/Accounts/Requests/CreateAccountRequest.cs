namespace ArgenCash.Application.DTOs.Accounts.Requests
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
