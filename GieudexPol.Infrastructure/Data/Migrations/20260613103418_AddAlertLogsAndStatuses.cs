using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertLogsAndStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTradingAlerts_IsActive_TradingPairId_EventType",
                table: "UserTradingAlerts");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserTradingAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE UserTradingAlerts
                SET Status = CASE
                    WHEN TriggeredDate IS NOT NULL THEN 1
                    WHEN IsActive = 1 THEN 0
                    ELSE 2
                END;

                UPDATE UserAlerts
                SET Status = CASE
                    WHEN TriggeredDate IS NOT NULL THEN 1
                    WHEN IsActive = 1 THEN 0
                    ELSE 2
                END;
                """);

            migrationBuilder.DropColumn(
                name: "AcknowledgedDate",
                table: "UserTradingAlerts");

            migrationBuilder.DropColumn(
                name: "IsAcknowledged",
                table: "UserTradingAlerts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserTradingAlerts");

            migrationBuilder.DropColumn(
                name: "AcknowledgedDate",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "IsAcknowledged",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserAlerts");

            migrationBuilder.CreateTable(
                name: "AlertLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAlertId = table.Column<int>(type: "int", nullable: true),
                    UserTradingAlertId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SourceSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertLogs_UserAlerts_UserAlertId",
                        column: x => x.UserAlertId,
                        principalTable: "UserAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertLogs_UserTradingAlerts_UserTradingAlertId",
                        column: x => x.UserTradingAlertId,
                        principalTable: "UserTradingAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTradingAlerts_Status_TradingPairId_EventType",
                table: "UserTradingAlerts",
                columns: new[] { "Status", "TradingPairId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertLogs_UserAlertId_CreatedDate",
                table: "AlertLogs",
                columns: new[] { "UserAlertId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertLogs_UserTradingAlertId_CreatedDate",
                table: "AlertLogs",
                columns: new[] { "UserTradingAlertId", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserTradingAlerts_Status_TradingPairId_EventType",
                table: "UserTradingAlerts");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedDate",
                table: "UserTradingAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAcknowledged",
                table: "UserTradingAlerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserTradingAlerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedDate",
                table: "UserAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAcknowledged",
                table: "UserAlerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserAlerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE UserTradingAlerts
                SET IsActive = CASE WHEN Status = 0 THEN 1 ELSE 0 END;

                UPDATE UserAlerts
                SET IsActive = CASE WHEN Status = 0 THEN 1 ELSE 0 END;
                """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserTradingAlerts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserAlerts");

            migrationBuilder.CreateIndex(
                name: "IX_UserTradingAlerts_IsActive_TradingPairId_EventType",
                table: "UserTradingAlerts",
                columns: new[] { "IsActive", "TradingPairId", "EventType" });
        }
    }
}
