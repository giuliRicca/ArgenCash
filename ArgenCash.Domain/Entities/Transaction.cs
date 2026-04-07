using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public string TransactionType { get; private set; } = string.Empty;
        public string Currency { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal ConvertedAmountUSD { get; private set; }
        public decimal ConvertedAmountARS { get; private set; }
        public Guid? ExchangeRateId { get; private set; }
        public DateTime TransactionDate { get; private set; }
        private Transaction() { }

        public static Transaction Create(Guid accountId, decimal amount, string currency, string transactionType, string description, decimal convertedAmountUSD, decimal convertedAmountARS, Guid? exchangeRateId)
        {
            var normalizedCurrency = NormalizeCurrencyCode(currency, nameof(currency));
            var normalizedTransactionType = TransactionTypes.Normalize(transactionType, nameof(transactionType));
            var normalizedDescription = NormalizeDescription(description);

            if (accountId == Guid.Empty)
                throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            if (convertedAmountUSD <= 0)
                throw new ArgumentException("Converted USD amount must be greater than zero.", nameof(convertedAmountUSD));
            if (convertedAmountARS <= 0)
                throw new ArgumentException("Converted ARS amount must be greater than zero.", nameof(convertedAmountARS));

            return new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                TransactionType = normalizedTransactionType,
                Currency = normalizedCurrency,
                Description = normalizedDescription,
                ConvertedAmountUSD = convertedAmountUSD,
                ConvertedAmountARS = convertedAmountARS,
                ExchangeRateId = exchangeRateId,
                TransactionDate = DateTime.UtcNow
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

        private static string NormalizeDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.", nameof(description));
            }

            var normalizedDescription = description.Trim();

            if (normalizedDescription.Length > 250)
            {
                throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
            }

            return normalizedDescription;
        }
    }
}
