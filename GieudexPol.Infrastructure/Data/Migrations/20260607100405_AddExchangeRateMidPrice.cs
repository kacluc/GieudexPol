using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateMidPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MidPrice",
                table: "ExchangeRates",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE ExchangeRates
                SET MidPrice = ROUND((BuyPrice + SellPrice) / 2, 4)
                WHERE MidPrice IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MidPrice",
                table: "ExchangeRates");
        }
    }
}
