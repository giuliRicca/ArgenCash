using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.DTOs;

public class LiveExchangeRateByTypeDto
{
    public ExchangeRateType RateType { get; init; }
    public LiveExchangeRateDto Rate { get; init; } = new();
}
