using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

public class ExchangeRateUsabilityPolicy : IExchangeRateUsabilityPolicy
{
    private static readonly TimeSpan StandardFreshnessWindow = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FiatWeekendFallbackWindow = TimeSpan.FromHours(72);

    public bool IsUsable(ExchangeRate exchangeRate, DateTime nowUtc)
    {
        var utcNow = nowUtc.Kind == DateTimeKind.Utc ? nowUtc : nowUtc.ToUniversalTime();
        var age = utcNow - exchangeRate.EffectiveDate;

        if (age < TimeSpan.Zero)
        {
            return true;
        }

        if (age <= StandardFreshnessWindow)
        {
            return true;
        }

        if (exchangeRate.RateType == ExchangeRateType.Crypto)
        {
            return false;
        }

        if (IsWeekend(utcNow) && age <= FiatWeekendFallbackWindow)
        {
            return true;
        }

        return false;
    }

    private static bool IsWeekend(DateTime utcNow)
    {
        return utcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}
