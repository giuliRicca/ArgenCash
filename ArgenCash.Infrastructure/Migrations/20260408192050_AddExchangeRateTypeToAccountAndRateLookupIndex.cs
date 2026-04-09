using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateTypeToAccountAndRateLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_RateType_EffectiveDate",
                table: "ExchangeRates");

            migrationBuilder.AlterColumn<string>(
                name: "RateType",
                table: "ExchangeRates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "ExchangeRateType",
                table: "Accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "OFFICIAL");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_BaseCurrency_TargetCurrency_RateType_Effectiv~",
                table: "ExchangeRates",
                columns: new[] { "BaseCurrency", "TargetCurrency", "RateType", "EffectiveDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_ExchangeRateType_Allowed",
                table: "Accounts",
                sql: "\"ExchangeRateType\" IN ('OFFICIAL', 'CCL', 'MEP', 'BLUE', 'CRYPTO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_BaseCurrency_TargetCurrency_RateType_Effectiv~",
                table: "ExchangeRates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_ExchangeRateType_Allowed",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ExchangeRateType",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "RateType",
                table: "ExchangeRates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_RateType_EffectiveDate",
                table: "ExchangeRates",
                columns: new[] { "RateType", "EffectiveDate" },
                unique: true);
        }
    }
}
