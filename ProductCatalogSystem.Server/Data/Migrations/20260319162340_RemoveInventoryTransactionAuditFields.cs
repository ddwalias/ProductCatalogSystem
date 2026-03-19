using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogSystem.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInventoryTransactionAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "InventoryTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "InventoryTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "InventoryTransactions",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
