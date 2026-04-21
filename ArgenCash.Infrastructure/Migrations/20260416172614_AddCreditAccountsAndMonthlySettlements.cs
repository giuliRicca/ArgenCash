using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditAccountsAndMonthlySettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "Accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "STANDARD");

            migrationBuilder.AddColumn<Guid>(
                name: "FundingAccountId",
                table: "Accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDayOfMonth",
                table: "Accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditCardStatementSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementYear = table.Column<int>(type: "integer", nullable: false),
                    StatementMonth = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TransferGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardStatementSettlements", x => x.Id);
                    table.CheckConstraint("CK_CreditCardStatementSettlements_Amount_Positive", "\"Amount\" > 0");
                    table.CheckConstraint("CK_CreditCardStatementSettlements_Currency_Length", "char_length(\"Currency\") = 3");
                    table.CheckConstraint("CK_CreditCardStatementSettlements_StatementMonth_Range", "\"StatementMonth\" BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_CreditCardStatementSettlements_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditCardStatementSettlements_Accounts_FundingAccountId",
                        column: x => x.FundingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditCardStatementSettlements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_FundingAccountId",
                table: "Accounts",
                column: "FundingAccountId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_AccountType_Allowed",
                table: "Accounts",
                sql: "\"AccountType\" IN ('STANDARD', 'CREDIT')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_CreditSettings_Consistency",
                table: "Accounts",
                sql: "(\"AccountType\" = 'CREDIT' AND \"FundingAccountId\" IS NOT NULL AND \"PaymentDayOfMonth\" BETWEEN 1 AND 28) OR (\"AccountType\" = 'STANDARD' AND \"FundingAccountId\" IS NULL AND \"PaymentDayOfMonth\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardStatementSettlements_CreditAccountId_StatementYea~",
                table: "CreditCardStatementSettlements",
                columns: new[] { "CreditAccountId", "StatementYear", "StatementMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardStatementSettlements_FundingAccountId",
                table: "CreditCardStatementSettlements",
                column: "FundingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardStatementSettlements_UserId",
                table: "CreditCardStatementSettlements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_FundingAccountId",
                table: "Accounts",
                column: "FundingAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_FundingAccountId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "CreditCardStatementSettlements");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_FundingAccountId",
                table: "Accounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_AccountType_Allowed",
                table: "Accounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_CreditSettings_Consistency",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "FundingAccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PaymentDayOfMonth",
                table: "Accounts");
        }
    }
}
