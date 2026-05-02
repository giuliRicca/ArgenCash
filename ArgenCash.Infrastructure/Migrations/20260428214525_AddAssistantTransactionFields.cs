using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantTransactionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Transactions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "MANUAL");

            migrationBuilder.CreateTable(
                name: "LearnedCategoryMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NormalizedKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnedCategoryMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnedCategoryMappings_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnedCategoryMappings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Source_Allowed",
                table: "Transactions",
                sql: "\"Source\" IN ('MANUAL', 'ASSISTANT_TEXT', 'ASSISTANT_VOICE')");

            migrationBuilder.CreateIndex(
                name: "IX_LearnedCategoryMappings_CategoryId",
                table: "LearnedCategoryMappings",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnedCategoryMappings_UserId_NormalizedKey_TransactionType",
                table: "LearnedCategoryMappings",
                columns: new[] { "UserId", "NormalizedKey", "TransactionType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnedCategoryMappings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Source_Allowed",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Transactions");
        }
    }
}
