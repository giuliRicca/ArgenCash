namespace ArgenCash.Application.DTOs.Transactions.Responses;

public class MonthlyTransactionSummaryDto
{
    public decimal IncomeUsd { get; init; }
    public decimal ExpenseUsd { get; init; }
    public decimal NetUsd { get; init; }
}
