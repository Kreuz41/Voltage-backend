using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FP.Migrations
{
    /// <inheritdoc />
    public partial class transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_FromWalletId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_ToWalletId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "ToWalletId",
                table: "Transactions",
                newName: "ToUserId");

            migrationBuilder.RenameColumn(
                name: "FromWalletId",
                table: "Transactions",
                newName: "FromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ToWalletId",
                table: "Transactions",
                newName: "IX_Transactions_ToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_FromWalletId",
                table: "Transactions",
                newName: "IX_Transactions_FromUserId");

            migrationBuilder.AddColumn<decimal>(
                name: "DealSum",
                table: "Transactions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ToAgent",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_FromUserId",
                table: "Transactions",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_ToUserId",
                table: "Transactions",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_FromUserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_ToUserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DealSum",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ToAgent",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "ToUserId",
                table: "Transactions",
                newName: "ToWalletId");

            migrationBuilder.RenameColumn(
                name: "FromUserId",
                table: "Transactions",
                newName: "FromWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ToUserId",
                table: "Transactions",
                newName: "IX_Transactions_ToWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_FromUserId",
                table: "Transactions",
                newName: "IX_Transactions_FromWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_FromWalletId",
                table: "Transactions",
                column: "FromWalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_ToWalletId",
                table: "Transactions",
                column: "ToWalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
