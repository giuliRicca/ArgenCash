using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Domain.Entities
{
    public class Account
    {
        private const int MaxNameLength = 100;

        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string CurrencyCode { get; private set; } = string.Empty;
        public ExchangeRateType ExchangeRateType { get; private set; } = ExchangeRateType.Official;
        public DateTime CreatedAt { get; private set; }

        private Account() { }

        public static Account Create(string name, string currencyCode, Guid userId)
        {
            var normalizedName = NormalizeName(name);
            var normalizedCurrencyCode = NormalizeCurrencyCode(currencyCode, nameof(currencyCode));

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            return new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = normalizedName,
                CurrencyCode = normalizedCurrencyCode,
                ExchangeRateType = ExchangeRateType.Official,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Rename(string name)
        {
            Name = NormalizeName(name);
        }

        public void SetExchangeRateType(ExchangeRateType exchangeRateType)
        {
            ExchangeRateType = exchangeRateType;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Account name is required.", nameof(name));
            }

            var normalizedName = name.Trim();

            if (normalizedName.Length > MaxNameLength)
            {
                throw new ArgumentException($"Account name cannot exceed {MaxNameLength} characters.", nameof(name));
            }

            return normalizedName;
        }

        private static string NormalizeCurrencyCode(string currencyCode, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Currency code is required.", parameterName);
            }

            var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();

            if (normalizedCurrencyCode.Length != 3 || !normalizedCurrencyCode.All(char.IsLetter))
            {
                throw new ArgumentException("Currency code must be a 3-letter ISO code.", parameterName);
            }

            return normalizedCurrencyCode;
        }
    }
}
