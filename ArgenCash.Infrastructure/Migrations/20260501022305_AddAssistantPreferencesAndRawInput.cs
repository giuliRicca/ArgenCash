using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantPreferencesAndRawInput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssistantRawInput",
                table: "Transactions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssistantPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultExpenseAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultIncomeAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistantPreferences_Accounts_DefaultExpenseAccountId",
                        column: x => x.DefaultExpenseAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssistantPreferences_Accounts_DefaultIncomeAccountId",
                        column: x => x.DefaultIncomeAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssistantPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistantPreferences_DefaultExpenseAccountId",
                table: "AssistantPreferences",
                column: "DefaultExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistantPreferences_DefaultIncomeAccountId",
                table: "AssistantPreferences",
                column: "DefaultIncomeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistantPreferences_UserId",
                table: "AssistantPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssistantPreferences");

            migrationBuilder.DropColumn(
                name: "AssistantRawInput",
                table: "Transactions");
        }
    }
}
