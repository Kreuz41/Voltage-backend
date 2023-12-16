using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class balanceagentincome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BalanceInternal",
                table: "Users",
                newName: "BalanceIncome");

            migrationBuilder.RenameColumn(
                name: "BalanceFiat",
                table: "Users",
                newName: "BalanceAgent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BalanceIncome",
                table: "Users",
                newName: "BalanceInternal");

            migrationBuilder.RenameColumn(
                name: "BalanceAgent",
                table: "Users",
                newName: "BalanceFiat");
        }
    }
}
