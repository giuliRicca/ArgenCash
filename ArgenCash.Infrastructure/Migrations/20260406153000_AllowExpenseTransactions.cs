using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    public partial class AllowExpenseTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Amount_Positive",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonNegative",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonNegative",
                table: "Transactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Amount_NonZero",
                table: "Transactions",
                sql: "\"Amount\" <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonZero",
                table: "Transactions",
                sql: "\"ConvertedAmountUSD\" <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonZero",
                table: "Transactions",
                sql: "\"ConvertedAmountARS\" <> 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmounts_SameSign",
                table: "Transactions",
                sql: "((\"Amount\" > 0 AND \"ConvertedAmountUSD\" > 0 AND \"ConvertedAmountARS\" > 0) OR (\"Amount\" < 0 AND \"ConvertedAmountUSD\" < 0 AND \"ConvertedAmountARS\" < 0))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Amount_NonZero",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonZero",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonZero",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmounts_SameSign",
                table: "Transactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Amount_Positive",
                table: "Transactions",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_NonNegative",
                table: "Transactions",
                sql: "\"ConvertedAmountUSD\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_NonNegative",
                table: "Transactions",
                sql: "\"ConvertedAmountARS\" >= 0");
        }
    }
}
