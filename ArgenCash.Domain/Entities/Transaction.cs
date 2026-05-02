using System;

namespace ArgenCash.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType TransactionType { get; private set; }
        public string Currency { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal ConvertedAmountUSD { get; private set; }
        public decimal ConvertedAmountARS { get; private set; }
        public Guid? ExchangeRateId { get; private set; }
        public Guid? TransferGroupId { get; private set; }
        public Guid? CounterpartyAccountId { get; private set; }
        public Guid? CategoryId { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public TransactionSource Source { get; private set; }
        public string? AssistantRawInput { get; private set; }
        private Transaction() { }

        public static Transaction Create(Guid accountId, decimal amount, string currency, TransactionType transactionType, string? description, decimal convertedAmountUSD, decimal convertedAmountARS, Guid? exchangeRateId, Guid? categoryId = null, Guid? transferGroupId = null, Guid? counterpartyAccountId = null, DateTime? transactionDate = null, TransactionSource source = TransactionSource.Manual, string? assistantRawInput = null)
        {
            var normalizedCurrency = NormalizeCurrencyCode(currency, nameof(currency));
            var normalizedDescription = NormalizeDescription(description);
            var normalizedAssistantRawInput = NormalizeAssistantRawInput(assistantRawInput);

            if (accountId == Guid.Empty)
                throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            if (convertedAmountUSD <= 0)
                throw new ArgumentException("Converted USD amount must be greater than zero.", nameof(convertedAmountUSD));
            if (convertedAmountARS <= 0)
                throw new ArgumentException("Converted ARS amount must be greater than zero.", nameof(convertedAmountARS));
            if (exchangeRateId == Guid.Empty)
                throw new ArgumentException("Exchange rate id cannot be empty.", nameof(exchangeRateId));
            if (categoryId == Guid.Empty)
                throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));
            if (transferGroupId == Guid.Empty)
                throw new ArgumentException("Transfer group id cannot be empty.", nameof(transferGroupId));
            if (counterpartyAccountId == Guid.Empty)
                throw new ArgumentException("Counterparty account id cannot be empty.", nameof(counterpartyAccountId));
            if (counterpartyAccountId.HasValue && counterpartyAccountId.Value == accountId)
                throw new ArgumentException("Counterparty account id must be different from account id.", nameof(counterpartyAccountId));

            return new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                TransactionType = transactionType,
                Currency = normalizedCurrency,
                Description = normalizedDescription,
                ConvertedAmountUSD = convertedAmountUSD,
                ConvertedAmountARS = convertedAmountARS,
                ExchangeRateId = exchangeRateId,
                TransferGroupId = transferGroupId,
                CounterpartyAccountId = counterpartyAccountId,
                CategoryId = categoryId,
                TransactionDate = transactionDate ?? DateTime.UtcNow,
                Source = source,
                AssistantRawInput = normalizedAssistantRawInput
            };
        }

        public void UpdateDetails(
            decimal amount,
            string currency,
            Guid? categoryId,
            decimal convertedAmountUSD,
            decimal convertedAmountARS,
            Guid? exchangeRateId)
        {
            var normalizedCurrency = NormalizeCurrencyCode(currency, nameof(currency));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            if (convertedAmountUSD <= 0)
                throw new ArgumentException("Converted USD amount must be greater than zero.", nameof(convertedAmountUSD));
            if (convertedAmountARS <= 0)
                throw new ArgumentException("Converted ARS amount must be greater than zero.", nameof(convertedAmountARS));
            if (exchangeRateId == Guid.Empty)
                throw new ArgumentException("Exchange rate id cannot be empty.", nameof(exchangeRateId));
            if (categoryId == Guid.Empty)
                throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));

            Amount = amount;
            Currency = normalizedCurrency;
            CategoryId = categoryId;
            ConvertedAmountUSD = convertedAmountUSD;
            ConvertedAmountARS = convertedAmountARS;
            ExchangeRateId = exchangeRateId;
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

        private static string NormalizeDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            var normalizedDescription = description.Trim();

            if (normalizedDescription.Length > 250)
            {
                throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
            }

            return normalizedDescription;
        }

        private static string? NormalizeAssistantRawInput(string? assistantRawInput)
        {
            if (string.IsNullOrWhiteSpace(assistantRawInput))
            {
                return null;
            }

            var normalizedAssistantRawInput = assistantRawInput.Trim();
            if (normalizedAssistantRawInput.Length > 1000)
            {
                throw new ArgumentException("Assistant raw input cannot exceed 1000 characters.", nameof(assistantRawInput));
            }

            return normalizedAssistantRawInput;
        }
    }
}
