using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Interfaces;

public interface IExchangeRateUsabilityPolicy
{
    bool IsUsable(ExchangeRate exchangeRate, DateTime nowUtc);
}
