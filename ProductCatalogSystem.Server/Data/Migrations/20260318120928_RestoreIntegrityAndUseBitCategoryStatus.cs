using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogSystem.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestoreIntegrityAndUseBitCategoryStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Categories]
                SET [Status] = CASE
                    WHEN [Status] = 1 THEN 1
                    ELSE 0
                END;
                """);

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.Sql(
                """
                CREATE OR ALTER TRIGGER [TR_Products_RequireActiveCategory]
                ON [Products]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS i
                        INNER JOIN [Categories] AS c ON c.[Id] = i.[CategoryId]
                        WHERE c.[Status] = CAST(0 AS bit)
                    )
                    BEGIN
                        THROW 51001, 'Inactive categories cannot be used for product assignment.', 1;
                    END
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR ALTER TRIGGER [TR_Categories_PreventCycles]
                ON [Categories]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted
                        WHERE [ParentCategoryId] = [Id]
                    )
                    BEGIN
                        THROW 51002, 'The selected parent category would create a cycle.', 1;
                    END

                    DECLARE @CycleDetected bit = 0;

                    ;WITH [ParentChain] AS (
                        SELECT
                            i.[Id] AS [StartId],
                            i.[ParentCategoryId] AS [CurrentParentId]
                        FROM inserted AS i
                        WHERE i.[ParentCategoryId] IS NOT NULL

                        UNION ALL

                        SELECT
                            pc.[StartId],
                            c.[ParentCategoryId] AS [CurrentParentId]
                        FROM [ParentChain] AS pc
                        INNER JOIN [Categories] AS c ON c.[Id] = pc.[CurrentParentId]
                        WHERE pc.[CurrentParentId] IS NOT NULL
                    )
                    SELECT TOP (1) @CycleDetected = 1
                    FROM [ParentChain]
                    WHERE [CurrentParentId] = [StartId]
                    OPTION (MAXRECURSION 32767);

                    IF @CycleDetected = 1
                    BEGIN
                        THROW 51002, 'The selected parent category would create a cycle.', 1;
                    END
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[TR_Categories_PreventCycles]', N'TR') IS NOT NULL
                BEGIN
                    DROP TRIGGER [TR_Categories_PreventCycles];
                END;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[TR_Products_RequireActiveCategory]', N'TR') IS NOT NULL
                BEGIN
                    DROP TRIGGER [TR_Products_RequireActiveCategory];
                END;
                """);

            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "Categories",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.Sql(
                """
                UPDATE [Categories]
                SET [Status] = CASE
                    WHEN [Status] = 1 THEN 1
                    ELSE 2
                END;
                """);
        }
    }
}
