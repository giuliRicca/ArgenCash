using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArchitectureHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ExchangeRateId",
                table: "Transactions",
                column: "ExchangeRateId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Amount_Positive",
                table: "Transactions",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonNegative",
                table: "Transactions",
                sql: "\"ConvertedAmountARS\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonNegative",
                table: "Transactions",
                sql: "\"ConvertedAmountUSD\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Currency_Length",
                table: "Transactions",
                sql: "char_length(\"Currency\") = 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExchangeRates_BaseCurrency_Length",
                table: "ExchangeRates",
                sql: "char_length(\"BaseCurrency\") = 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExchangeRates_BuyPrice_Lte_SellPrice",
                table: "ExchangeRates",
                sql: "\"BuyPrice\" <= \"SellPrice\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExchangeRates_DifferentCurrencies",
                table: "ExchangeRates",
                sql: "\"BaseCurrency\" <> \"TargetCurrency\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExchangeRates_PositivePrices",
                table: "ExchangeRates",
                sql: "\"BuyPrice\" > 0 AND \"SellPrice\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExchangeRates_TargetCurrency_Length",
                table: "ExchangeRates",
                sql: "char_length(\"TargetCurrency\") = 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_CurrencyCode_Length",
                table: "Accounts",
                sql: "char_length(\"CurrencyCode\") = 3");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExchangeRates_ExchangeRateId",
                table: "Transactions",
                column: "ExchangeRateId",
                principalTable: "ExchangeRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExchangeRates_ExchangeRateId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ExchangeRateId",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Amount_Positive",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonNegative",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonNegative",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Currency_Length",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExchangeRates_BaseCurrency_Length",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExchangeRates_BuyPrice_Lte_SellPrice",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExchangeRates_DifferentCurrencies",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExchangeRates_PositivePrices",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExchangeRates_TargetCurrency_Length",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_CurrencyCode_Length",
                table: "Accounts");
        }
    }
}
