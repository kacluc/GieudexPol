using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignUserAlertSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UserAlerts",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "TargetPrice",
                table: "UserAlerts",
                newName: "ThresholdValue");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThresholdValue",
                table: "UserAlerts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "AlertType",
                table: "UserAlerts",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentageChange",
                table: "UserAlerts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeFrameHours",
                table: "UserAlerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TriggeredDate",
                table: "UserAlerts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "PercentageChange",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "TimeFrameHours",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "TriggeredDate",
                table: "UserAlerts");

            migrationBuilder.Sql(
                "UPDATE [UserAlerts] SET [ThresholdValue] = 0 WHERE [ThresholdValue] IS NULL;");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThresholdValue",
                table: "UserAlerts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "ThresholdValue",
                table: "UserAlerts",
                newName: "TargetPrice");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "UserAlerts",
                newName: "CreatedAt");
        }
    }
}
