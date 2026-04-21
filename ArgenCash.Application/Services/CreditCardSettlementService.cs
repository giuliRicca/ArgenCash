using ArgenCash.Application.Interfaces;
using ArgenCash.Domain.Entities;

namespace ArgenCash.Application.Services;

/// <summary>
/// Executes monthly settlement transfers for credit accounts.
/// </summary>
public class CreditCardSettlementService(
    IAccountRepository accountRepository,
    ICreditCardStatementSettlementRepository settlementRepository,
    IExchangeRateResolver exchangeRateResolver) : ICreditCardSettlementService
{
    private readonly IAccountRepository _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    private readonly ICreditCardStatementSettlementRepository _settlementRepository = settlementRepository ?? throw new ArgumentNullException(nameof(settlementRepository));
    private readonly IExchangeRateResolver _exchangeRateResolver = exchangeRateResolver ?? throw new ArgumentNullException(nameof(exchangeRateResolver));

    /// <inheritdoc />
    public async Task<CreditSettlementProcessResultDto> ProcessDueSettlementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        var statementMonthDate = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var fromUtc = statementMonthDate;
        var toUtcExclusive = statementMonthDate.AddMonths(1);

        var candidates = await _accountRepository.GetCreditSettlementCandidatesAsync(userId, cancellationToken);

        var processedCount = 0;
        var skippedCount = 0;

        foreach (var candidate in candidates)
        {
            if (nowUtc.Day < candidate.PaymentDayOfMonth)
            {
                skippedCount++;
                continue;
            }

            var alreadyProcessed = await _settlementRepository.ExistsAsync(
                candidate.Id,
                statementMonthDate.Year,
                statementMonthDate.Month,
                cancellationToken);

            if (alreadyProcessed)
            {
                skippedCount++;
                continue;
            }

            var amount = await _accountRepository.GetCreditStatementNetExpenseAsync(
                candidate.Id,
                fromUtc,
                toUtcExclusive,
                cancellationToken);

            if (amount <= 0)
            {
                skippedCount++;
                continue;
            }

            var fromAccount = await _accountRepository.GetForUpdateAsync(candidate.FundingAccountId, userId, cancellationToken);
            var toAccount = await _accountRepository.GetForUpdateAsync(candidate.Id, userId, cancellationToken);

            if (fromAccount is null || toAccount is null)
            {
                skippedCount++;
                continue;
            }

            var resolvedRate = await _exchangeRateResolver.ResolveAsync("USD", "ARS", fromAccount.ExchangeRateType, cancellationToken);
            var (convertedAmountUsd, convertedAmountArs) = CalculateConvertedAmounts(
                amount,
                candidate.CurrencyCode,
                resolvedRate.BuyPrice,
                resolvedRate.SellPrice);

            var transferGroupId = Guid.NewGuid();
            var statementLabel = $"{statementMonthDate:yyyy-MM}";
            var description = $"Credit statement payment {statementLabel}";

            var fromTransaction = Transaction.Create(
                candidate.FundingAccountId,
                amount,
                candidate.CurrencyCode,
                TransactionType.Expense,
                description,
                convertedAmountUsd,
                convertedAmountArs,
                resolvedRate.Id,
                null,
                transferGroupId,
                candidate.Id);

            var toTransaction = Transaction.Create(
                candidate.Id,
                amount,
                candidate.CurrencyCode,
                TransactionType.Income,
                description,
                convertedAmountUsd,
                convertedAmountArs,
                resolvedRate.Id,
                null,
                transferGroupId,
                candidate.FundingAccountId);

            var settlement = CreditCardStatementSettlement.Create(
                userId,
                candidate.Id,
                candidate.FundingAccountId,
                statementMonthDate.Year,
                statementMonthDate.Month,
                amount,
                candidate.CurrencyCode,
                transferGroupId);

            await _accountRepository.AddTransactionAsync(fromTransaction);
            await _accountRepository.AddTransactionAsync(toTransaction);
            await _settlementRepository.AddAsync(settlement, cancellationToken);
            await _accountRepository.SaveChangesAsync();

            processedCount++;
        }

        return new CreditSettlementProcessResultDto
        {
            ProcessedCount = processedCount,
            SkippedCount = skippedCount
        };
    }

    private static (decimal convertedAmountUsd, decimal convertedAmountArs) CalculateConvertedAmounts(
        decimal amount,
        string currency,
        decimal buyRate,
        decimal sellRate)
    {
        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        return normalizedCurrency switch
        {
            "USD" => (amount, amount * buyRate),
            "ARS" => (amount / sellRate, amount),
            _ => throw new ArgumentException("Currency must be USD or ARS.", nameof(currency)),
        };
    }
}
