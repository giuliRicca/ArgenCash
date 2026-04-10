namespace ArgenCash.Domain.Entities;

public static class ExchangeRateTypes
{
    public static ExchangeRateType ToEnum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Exchange rate type is required.", nameof(value));

        return value.Trim().ToUpperInvariant() switch
        {
            "OFFICIAL" => ExchangeRateType.Official,
            "CCL" => ExchangeRateType.Ccl,
            "MEP" => ExchangeRateType.Mep,
            "BLUE" => ExchangeRateType.Blue,
            "CRYPTO" => ExchangeRateType.Crypto,
            _ => throw new ArgumentException("Unsupported exchange rate type.", nameof(value))
        };
    }

    public static string ToString(ExchangeRateType value)
    {
        return value switch
        {
            ExchangeRateType.Official => "OFFICIAL",
            ExchangeRateType.Ccl => "CCL",
            ExchangeRateType.Mep => "MEP",
            ExchangeRateType.Blue => "BLUE",
            ExchangeRateType.Crypto => "CRYPTO",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported exchange rate type.")
        };
    }
}
