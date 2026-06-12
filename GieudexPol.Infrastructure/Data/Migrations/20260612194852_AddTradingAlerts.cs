using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTradingAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TradingPairId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    TargetPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTradingAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTradingAlerts_TradingPairs_TradingPairId",
                        column: x => x.TradingPairId,
                        principalTable: "TradingPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTradingAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTradingAlerts_IsActive_TradingPairId_EventType",
                table: "UserTradingAlerts",
                columns: new[] { "IsActive", "TradingPairId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTradingAlerts_TradingPairId",
                table: "UserTradingAlerts",
                column: "TradingPairId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTradingAlerts_UserId",
                table: "UserTradingAlerts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTradingAlerts");
        }
    }
}
