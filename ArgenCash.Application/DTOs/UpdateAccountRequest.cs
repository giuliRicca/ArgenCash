using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs;

public class UpdateAccountRequest
{
    public string? Name { get; set; }
    public ExchangeRateType? ExchangeRateType { get; set; }
}
