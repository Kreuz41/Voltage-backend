using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class VerificationsCodesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_Promocodes_CodeId",
                table: "Investments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promocodes",
                table: "Promocodes");

            migrationBuilder.RenameTable(
                name: "Promocodes",
                newName: "PromoCodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromoCodes",
                table: "PromoCodes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_PromoCodes_CodeId",
                table: "Investments",
                column: "CodeId",
                principalTable: "PromoCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_PromoCodes_CodeId",
                table: "Investments");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromoCodes",
                table: "PromoCodes");

            migrationBuilder.RenameTable(
                name: "PromoCodes",
                newName: "Promocodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promocodes",
                table: "Promocodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_Promocodes_CodeId",
                table: "Investments",
                column: "CodeId",
                principalTable: "Promocodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
