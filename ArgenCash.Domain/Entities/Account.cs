using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Domain.Entities
{
    /// <summary>
    /// Represents a user-owned financial account used to record transactions.
    /// </summary>
    public class Account
    {
        private const int MaxNameLength = 100;
        private const int MinimumPaymentDayOfMonth = 1;
        private const int MaximumPaymentDayOfMonth = 28;

        /// <summary>
        /// Gets the account identifier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the account owner identifier.
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the three-letter ISO currency code.
        /// </summary>
        public string CurrencyCode { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the account-level exchange rate preference.
        /// </summary>
        public ExchangeRateType ExchangeRateType { get; private set; } = ExchangeRateType.Official;

        /// <summary>
        /// Gets the account behavior type.
        /// </summary>
        public AccountType AccountType { get; private set; } = AccountType.Standard;

        /// <summary>
        /// Gets the funding account identifier used for credit statement payment.
        /// </summary>
        public Guid? FundingAccountId { get; private set; }

        /// <summary>
        /// Gets the month day used to trigger credit statement payment.
        /// </summary>
        public int? PaymentDayOfMonth { get; private set; }

        /// <summary>
        /// Gets the UTC creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        private Account() { }

        /// <summary>
        /// Creates a new account instance.
        /// </summary>
        /// <param name="name">Account display name.</param>
        /// <param name="currencyCode">Three-letter ISO currency code.</param>
        /// <param name="userId">Owner identifier.</param>
        /// <param name="accountType">Account behavior type.</param>
        /// <param name="fundingAccountId">Funding account used by credit accounts.</param>
        /// <param name="paymentDayOfMonth">Settlement day used by credit accounts.</param>
        /// <returns>A configured <see cref="Account"/> instance.</returns>
        public static Account Create(
            string name,
            string currencyCode,
            Guid userId,
            AccountType accountType = AccountType.Standard,
            Guid? fundingAccountId = null,
            int? paymentDayOfMonth = null)
        {
            var normalizedName = NormalizeName(name);
            var normalizedCurrencyCode = NormalizeCurrencyCode(currencyCode, nameof(currencyCode));

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            ValidateAccountTypeSettings(accountType, fundingAccountId, paymentDayOfMonth, Guid.Empty);

            return new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = normalizedName,
                CurrencyCode = normalizedCurrencyCode,
                ExchangeRateType = ExchangeRateType.Official,
                AccountType = accountType,
                FundingAccountId = fundingAccountId,
                PaymentDayOfMonth = paymentDayOfMonth,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the account name.
        /// </summary>
        /// <param name="name">New display name.</param>
        public void Rename(string name)
        {
            Name = NormalizeName(name);
        }

        /// <summary>
        /// Sets the exchange rate preference for this account.
        /// </summary>
        /// <param name="exchangeRateType">Selected exchange rate type.</param>
        public void SetExchangeRateType(ExchangeRateType exchangeRateType)
        {
            ExchangeRateType = exchangeRateType;
        }

        /// <summary>
        /// Applies account-type specific settings.
        /// </summary>
        /// <param name="accountType">Account behavior type.</param>
        /// <param name="fundingAccountId">Funding account used by credit accounts.</param>
        /// <param name="paymentDayOfMonth">Settlement day used by credit accounts.</param>
        public void ConfigureAccountType(AccountType accountType, Guid? fundingAccountId, int? paymentDayOfMonth)
        {
            ValidateAccountTypeSettings(accountType, fundingAccountId, paymentDayOfMonth, Id);

            AccountType = accountType;
            FundingAccountId = accountType == AccountType.Credit ? fundingAccountId : null;
            PaymentDayOfMonth = accountType == AccountType.Credit ? paymentDayOfMonth : null;
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

        private static void ValidateAccountTypeSettings(
            AccountType accountType,
            Guid? fundingAccountId,
            int? paymentDayOfMonth,
            Guid accountId)
        {
            if (accountType == AccountType.Credit)
            {
                if (!fundingAccountId.HasValue)
                {
                    throw new ArgumentException("Funding account is required for credit accounts.", nameof(fundingAccountId));
                }

                if (fundingAccountId.Value == Guid.Empty)
                {
                    throw new ArgumentException("Funding account id cannot be empty.", nameof(fundingAccountId));
                }

                if (paymentDayOfMonth is null)
                {
                    throw new ArgumentException("Payment day is required for credit accounts.", nameof(paymentDayOfMonth));
                }

                if (paymentDayOfMonth < MinimumPaymentDayOfMonth || paymentDayOfMonth > MaximumPaymentDayOfMonth)
                {
                    throw new ArgumentException(
                        $"Payment day must be between {MinimumPaymentDayOfMonth} and {MaximumPaymentDayOfMonth}.",
                        nameof(paymentDayOfMonth));
                }

                if (accountId != Guid.Empty && fundingAccountId == accountId)
                {
                    throw new ArgumentException("Funding account must be different from the credit account.", nameof(fundingAccountId));
                }

                return;
            }

            if (fundingAccountId.HasValue)
            {
                throw new ArgumentException("Funding account can only be set for credit accounts.", nameof(fundingAccountId));
            }

            if (paymentDayOfMonth.HasValue)
            {
                throw new ArgumentException("Payment day can only be set for credit accounts.", nameof(paymentDayOfMonth));
            }
        }
    }
}
