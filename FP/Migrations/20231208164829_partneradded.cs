using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class partneradded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "Operations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_PartnerId",
                table: "Operations",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations",
                column: "PartnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_PartnerId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Operations");
        }
    }
}
