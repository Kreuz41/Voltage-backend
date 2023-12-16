using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class withdrawstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Investments_CodeId",
                table: "Investments");

            migrationBuilder.RenameColumn(
                name: "IsRealized",
                table: "Withdraws",
                newName: "FromAgentBalance");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Withdraws",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Investments_CodeId",
                table: "Investments",
                column: "CodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Investments_CodeId",
                table: "Investments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Withdraws");

            migrationBuilder.RenameColumn(
                name: "FromAgentBalance",
                table: "Withdraws",
                newName: "IsRealized");

            migrationBuilder.CreateIndex(
                name: "IX_Investments_CodeId",
                table: "Investments",
                column: "CodeId",
                unique: true);
        }
    }
}
