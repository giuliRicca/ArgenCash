namespace ArgenCash.Domain.Entities;

/// <summary>
/// Converts account type values between persistence and domain representations.
/// </summary>
public static class AccountTypes
{
    /// <summary>
    /// Converts the persisted account type string to <see cref="AccountType"/>.
    /// </summary>
    /// <param name="value">Persisted account type value.</param>
    /// <returns>The matching <see cref="AccountType"/> value.</returns>
    public static AccountType ToEnum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Account type is required.", nameof(value));
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "STANDARD" => AccountType.Standard,
            "CREDIT" => AccountType.Credit,
            _ => throw new ArgumentException("Unsupported account type.", nameof(value))
        };
    }

    /// <summary>
    /// Converts an <see cref="AccountType"/> to its persisted string representation.
    /// </summary>
    /// <param name="value">The account type value.</param>
    /// <returns>Uppercase storage representation.</returns>
    public static string ToString(AccountType value)
    {
        return value switch
        {
            AccountType.Standard => "STANDARD",
            AccountType.Credit => "CREDIT",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported account type.")
        };
    }
}
