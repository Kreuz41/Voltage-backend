using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class transactionsfromagent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FromAgent",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromAgent",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Transactions");
        }
    }
}
