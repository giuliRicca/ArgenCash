using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgenCash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTransferFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CounterpartyAccountId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferGroupId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CounterpartyAccountId",
                table: "Transactions",
                column: "CounterpartyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferGroupId",
                table: "Transactions",
                column: "TransferGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_CounterpartyAccountId",
                table: "Transactions",
                column: "CounterpartyAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_CounterpartyAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CounterpartyAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransferGroupId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CounterpartyAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransferGroupId",
                table: "Transactions");
        }
    }
}
