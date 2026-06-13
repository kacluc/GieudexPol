using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkTransactionsToTradeExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TradeExecutionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TradeExecutionId",
                table: "Transactions",
                column: "TradeExecutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TradeExecutions_TradeExecutionId",
                table: "Transactions",
                column: "TradeExecutionId",
                principalTable: "TradeExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TradeExecutions_TradeExecutionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TradeExecutionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TradeExecutionId",
                table: "Transactions");
        }
    }
}
