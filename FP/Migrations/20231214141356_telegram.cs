using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class telegram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "VerificationCodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TelegramId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserId",
                table: "VerificationCodes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationCodes_Users_UserId",
                table: "VerificationCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerificationCodes_Users_UserId",
                table: "VerificationCodes");

            migrationBuilder.DropIndex(
                name: "IX_VerificationCodes_UserId",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "TelegramId",
                table: "Users");
        }
    }
}
