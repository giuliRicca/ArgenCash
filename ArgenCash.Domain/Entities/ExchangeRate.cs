using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Domain.Entities
{
    public class ExchangeRate
    {
        public Guid Id { get; private set; }
        public ExchangeRateType RateType { get; private set; }
        public string BaseCurrency { get; private set; } = string.Empty;
        public string TargetCurrency { get; private set; } = string.Empty;
        public decimal BuyPrice { get; private set; }
        public decimal SellPrice { get; private set; }
        public DateTime EffectiveDate { get; private set; }

        private ExchangeRate() { }

        public static ExchangeRate Create(string rateType, string baseCurrency, string targetCurrency, decimal buyPrice, decimal sellPrice, DateTime effectiveDate)
        {
            return Create(ExchangeRateTypes.ToEnum(rateType), baseCurrency, targetCurrency, buyPrice, sellPrice, effectiveDate);
        }

        public static ExchangeRate Create(ExchangeRateType rateType, string baseCurrency, string targetCurrency, decimal buyPrice, decimal sellPrice, DateTime effectiveDate)
        {
            var normalizedBaseCurrency = NormalizeCurrencyCode(baseCurrency, nameof(baseCurrency));
            var normalizedTargetCurrency = NormalizeCurrencyCode(targetCurrency, nameof(targetCurrency));
            var normalizedEffectiveDate = NormalizeEffectiveDate(effectiveDate);

            if (normalizedBaseCurrency == normalizedTargetCurrency)
            {
                throw new ArgumentException("Base currency and target currency must be different.", nameof(targetCurrency));
            }

            if (buyPrice <= 0)
            {
                throw new ArgumentException("Buy price must be greater than zero.", nameof(buyPrice));
            }

            if (sellPrice <= 0)
            {
                throw new ArgumentException("Sell price must be greater than zero.", nameof(sellPrice));
            }

            if (buyPrice > sellPrice)
            {
                throw new ArgumentException("Buy price cannot be greater than sell price.", nameof(buyPrice));
            }

            return new ExchangeRate
            {
                Id = Guid.NewGuid(),
                RateType = rateType,
                BaseCurrency = normalizedBaseCurrency,
                TargetCurrency = normalizedTargetCurrency,
                BuyPrice = buyPrice,
                SellPrice = sellPrice,
                EffectiveDate = normalizedEffectiveDate
            };
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

        private static DateTime NormalizeEffectiveDate(DateTime effectiveDate)
        {
            if (effectiveDate == default)
            {
                throw new ArgumentException("Effective date is required.", nameof(effectiveDate));
            }

            return effectiveDate.Kind switch
            {
                DateTimeKind.Utc => effectiveDate,
                DateTimeKind.Local => effectiveDate.ToUniversalTime(),
                _ => DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc)
            };
        }
    }
}
