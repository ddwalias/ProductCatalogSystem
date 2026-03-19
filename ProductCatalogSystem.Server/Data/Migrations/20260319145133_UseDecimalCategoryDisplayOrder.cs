using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogSystem.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseDecimalCategoryDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId_DisplayOrder",
                table: "Categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Categories_DisplayOrder_NonNegative",
                table: "Categories");

            migrationBuilder.AlterColumn<decimal>(
                name: "DisplayOrder",
                table: "Categories",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Categories_DisplayOrder_NonNegative",
                table: "Categories",
                sql: "[DisplayOrder] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder",
                unique: true,
                filter: "[ParentCategoryId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId_DisplayOrder",
                table: "Categories",
                columns: new[] { "ParentCategoryId", "DisplayOrder" },
                unique: true,
                filter: "[ParentCategoryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId_DisplayOrder",
                table: "Categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Categories_DisplayOrder_NonNegative",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Categories_DisplayOrder_NonNegative",
                table: "Categories",
                sql: "[DisplayOrder] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder",
                unique: true,
                filter: "[ParentCategoryId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId_DisplayOrder",
                table: "Categories",
                columns: new[] { "ParentCategoryId", "DisplayOrder" },
                unique: true,
                filter: "[ParentCategoryId] IS NOT NULL");
        }
    }
}
