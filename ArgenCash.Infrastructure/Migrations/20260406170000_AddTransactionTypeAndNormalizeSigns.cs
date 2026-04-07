using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    public partial class AddTransactionTypeAndNormalizeSigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "INCOME");

            migrationBuilder.Sql("""
                UPDATE public."Transactions"
                SET "TransactionType" = CASE WHEN "Amount" < 0 THEN 'EXPENSE' ELSE 'INCOME' END,
                    "Amount" = ABS("Amount"),
                    "ConvertedAmountUSD" = ABS("ConvertedAmountUSD"),
                    "ConvertedAmountARS" = ABS("ConvertedAmountARS");
                """);

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
                name: "CK_Transactions_ConvertedAmountUSD_Positive",
                table: "Transactions",
                sql: "\"ConvertedAmountUSD\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_Positive",
                table: "Transactions",
                sql: "\"ConvertedAmountARS\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Type_Allowed",
                table: "Transactions",
                sql: "\"TransactionType\" IN ('INCOME', 'EXPENSE')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Amount_Positive",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountUSD_Positive",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_ConvertedAmountARS_Positive",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Type_Allowed",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
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
    }
}
