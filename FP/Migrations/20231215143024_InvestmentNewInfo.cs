using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class InvestmentNewInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinDuration",
                table: "PackTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxSum",
                table: "Investments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinDuration",
                table: "PackTypes");

            migrationBuilder.DropColumn(
                name: "MaxSum",
                table: "Investments");
        }
    }
}
