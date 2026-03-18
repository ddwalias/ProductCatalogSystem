using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogSystem.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryValidationTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.check_constraints
                    WHERE [name] = N'CK_Categories_DisplayOrder_NonNegative'
                      AND [parent_object_id] = OBJECT_ID(N'[Categories]')
                )
                BEGIN
                    ALTER TABLE [Categories]
                    ADD CONSTRAINT [CK_Categories_DisplayOrder_NonNegative]
                    CHECK ([DisplayOrder] >= 0);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.check_constraints
                    WHERE [name] = N'CK_Categories_DisplayOrder_NonNegative'
                      AND [parent_object_id] = OBJECT_ID(N'[Categories]')
                )
                BEGIN
                    ALTER TABLE [Categories]
                    DROP CONSTRAINT [CK_Categories_DisplayOrder_NonNegative];
                END;
                """);
        }
    }
}
