using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class OperationTypeFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Operations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations",
                column: "PartnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerId",
                table: "Operations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Users_PartnerId",
                table: "Operations",
                column: "PartnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
