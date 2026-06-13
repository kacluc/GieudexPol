using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAccountsTreasuryAndFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE [Users]
                SET [AccountType] =
                    CASE
                        WHEN [Role] = N'SuperAdmin' THEN 2
                        WHEN [Role] = N'Admin' THEN 1
                        ELSE 0
                    END;
                """);

            migrationBuilder.AddColumn<int>(
                name: "ExchangeExecutionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BuyerFee",
                table: "TradeExecutions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FeeCurrencyId",
                table: "TradeExecutions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerFee",
                table: "TradeExecutions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SystemUserId",
                table: "RateSources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExecutedQuoteAmount",
                table: "Orders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeePaid",
                table: "Orders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ExchangeExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RateSourceId = table.Column<int>(type: "int", nullable: false),
                    FromCurrencyId = table.Column<int>(type: "int", nullable: false),
                    ToCurrencyId = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeCurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeExecutions_Currencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeExecutions_Currencies_FromCurrencyId",
                        column: x => x.FromCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeExecutions_Currencies_ToCurrencyId",
                        column: x => x.ToCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeExecutions_RateSources_RateSourceId",
                        column: x => x.RateSourceId,
                        principalTable: "RateSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeExecutions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccountType",
                table: "Users",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ExchangeExecutionId",
                table: "Transactions",
                column: "ExchangeExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeExecutions_FeeCurrencyId",
                table: "TradeExecutions",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RateSources_SystemUserId",
                table: "RateSources",
                column: "SystemUserId",
                unique: true,
                filter: "[SystemUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeExecutions_FeeCurrencyId",
                table: "ExchangeExecutions",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeExecutions_FromCurrencyId",
                table: "ExchangeExecutions",
                column: "FromCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeExecutions_RateSourceId",
                table: "ExchangeExecutions",
                column: "RateSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeExecutions_ToCurrencyId",
                table: "ExchangeExecutions",
                column: "ToCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeExecutions_UserId",
                table: "ExchangeExecutions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RateSources_Users_SystemUserId",
                table: "RateSources",
                column: "SystemUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeExecutions_Currencies_FeeCurrencyId",
                table: "TradeExecutions",
                column: "FeeCurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExchangeExecutions_ExchangeExecutionId",
                table: "Transactions",
                column: "ExchangeExecutionId",
                principalTable: "ExchangeExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RateSources_Users_SystemUserId",
                table: "RateSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeExecutions_Currencies_FeeCurrencyId",
                table: "TradeExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExchangeExecutions_ExchangeExecutionId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "ExchangeExecutions");

            migrationBuilder.DropIndex(
                name: "IX_Users_AccountType",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ExchangeExecutionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TradeExecutions_FeeCurrencyId",
                table: "TradeExecutions");

            migrationBuilder.DropIndex(
                name: "IX_RateSources_SystemUserId",
                table: "RateSources");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExchangeExecutionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BuyerFee",
                table: "TradeExecutions");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                table: "TradeExecutions");

            migrationBuilder.DropColumn(
                name: "SellerFee",
                table: "TradeExecutions");

            migrationBuilder.DropColumn(
                name: "SystemUserId",
                table: "RateSources");

            migrationBuilder.DropColumn(
                name: "ExecutedQuoteAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FeePaid",
                table: "Orders");
        }
    }
}
