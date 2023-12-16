using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class source : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_PackTypes_PackTypeId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_PackTypeId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "PackTypeId",
                table: "Operations");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Operations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Operations");

            migrationBuilder.AddColumn<int>(
                name: "PackTypeId",
                table: "Operations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_PackTypeId",
                table: "Operations",
                column: "PackTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_PackTypes_PackTypeId",
                table: "Operations",
                column: "PackTypeId",
                principalTable: "PackTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
