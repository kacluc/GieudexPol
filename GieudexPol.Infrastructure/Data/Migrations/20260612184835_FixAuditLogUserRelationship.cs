using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GieudexPol.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditLogUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_Users_UserId1",
                table: "AuditLog");

            migrationBuilder.Sql(
                """
                UPDATE auditLog
                SET auditLog.UserId = users.AuthId
                FROM AuditLog AS auditLog
                INNER JOIN Users AS users ON users.Id = auditLog.UserId1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Users_AuthId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UserId1",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AuditLog");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_AuthId",
                table: "Users",
                column: "AuthId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Users_UserId",
                table: "AuditLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "AuthId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_Users_UserId",
                table: "AuditLog");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_AuthId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "AuditLog",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE auditLog
                SET auditLog.UserId1 = users.Id
                FROM AuditLog AS auditLog
                INNER JOIN Users AS users ON users.AuthId = auditLog.UserId;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "UserId1",
                table: "AuditLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthId",
                table: "Users",
                column: "AuthId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId1",
                table: "AuditLog",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Users_UserId1",
                table: "AuditLog",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
