using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProductCatalogSystem.Server.Data;

#nullable disable

namespace ProductCatalogSystem.Server.Data.Migrations;

[DbContext(typeof(CatalogDbContext))]
[Migration("20260318090000_RemoveValidationTriggers")]
public partial class RemoveValidationTriggers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[TR_Products_RequireActiveCategory]', N'TR') IS NOT NULL
            BEGIN
                DROP TRIGGER [TR_Products_RequireActiveCategory];
            END;
            """);

        migrationBuilder.Sql(
            """
            IF OBJECT_ID(N'[TR_Categories_PreventCycles]', N'TR') IS NOT NULL
            BEGIN
                DROP TRIGGER [TR_Categories_PreventCycles];
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
