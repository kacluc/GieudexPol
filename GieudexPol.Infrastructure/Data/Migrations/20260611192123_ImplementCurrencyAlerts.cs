using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImplementCurrencyAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceSide",
                table: "UserAlerts",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "RateSourceId",
                table: "UserAlerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThresholdDirection",
                table: "UserAlerts",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [UserAlerts] SET [ThresholdDirection] = 0 WHERE [AlertType] = 2;");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [Notifications] ([UserId], [Message], [CreatedDate], [IsRead])
                SELECT [UserId1], [Message], [CreatedAt], [IsRead]
                FROM [Notification]
                WHERE EXISTS (
                    SELECT 1 FROM [Users] WHERE [Users].[Id] = [Notification].[UserId1]
                );
                """);

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.CreateTable(
                name: "UserAlertEvaluationStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAlertId = table.Column<int>(type: "int", nullable: false),
                    RateSourceId = table.Column<int>(type: "int", nullable: false),
                    LastEvaluatedEffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAlertEvaluationStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAlertEvaluationStates_RateSources_RateSourceId",
                        column: x => x.RateSourceId,
                        principalTable: "RateSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAlertEvaluationStates_UserAlerts_UserAlertId",
                        column: x => x.UserAlertId,
                        principalTable: "UserAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAlerts_RateSourceId",
                table: "UserAlerts",
                column: "RateSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlertEvaluationStates_RateSourceId",
                table: "UserAlertEvaluationStates",
                column: "RateSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlertEvaluationStates_UserAlertId_RateSourceId",
                table: "UserAlertEvaluationStates",
                columns: new[] { "UserAlertId", "RateSourceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAlerts_RateSources_RateSourceId",
                table: "UserAlerts",
                column: "RateSourceId",
                principalTable: "RateSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAlerts_RateSources_RateSourceId",
                table: "UserAlerts");

            migrationBuilder.DropTable(
                name: "UserAlertEvaluationStates");

            migrationBuilder.DropIndex(
                name: "IX_UserAlerts_RateSourceId",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "PriceSide",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "RateSourceId",
                table: "UserAlerts");

            migrationBuilder.DropColumn(
                name: "ThresholdDirection",
                table: "UserAlerts");

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [Notification] (
                    [Id], [UserId], [Message], [IsRead], [CreatedAt], [UserId1])
                SELECT NEWID(), NEWID(), [Message], [IsRead], [CreatedDate], [UserId]
                FROM [Notifications];
                """);

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId1",
                table: "Notification",
                column: "UserId1");
        }
    }
}
