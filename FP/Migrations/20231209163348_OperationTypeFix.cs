using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class OperationTypeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_OperationTypes_OperationId",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "OperationId",
                table: "Operations",
                newName: "OperationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Operations_OperationId",
                table: "Operations",
                newName: "IX_Operations_OperationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_OperationTypes_OperationTypeId",
                table: "Operations",
                column: "OperationTypeId",
                principalTable: "OperationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_OperationTypes_OperationTypeId",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "OperationTypeId",
                table: "Operations",
                newName: "OperationId");

            migrationBuilder.RenameIndex(
                name: "IX_Operations_OperationTypeId",
                table: "Operations",
                newName: "IX_Operations_OperationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_OperationTypes_OperationId",
                table: "Operations",
                column: "OperationId",
                principalTable: "OperationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
