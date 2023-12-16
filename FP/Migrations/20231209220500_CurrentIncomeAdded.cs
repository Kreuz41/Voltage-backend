using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class CurrentIncomeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentIncome",
                table: "Users",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentIncome",
                table: "Users");
        }
    }
}
