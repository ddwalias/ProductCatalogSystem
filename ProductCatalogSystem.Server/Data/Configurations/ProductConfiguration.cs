using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", tableBuilder =>
        {
            tableBuilder.HasTrigger("TR_Products_RequireActiveCategory");
            tableBuilder.IsTemporal(temporalBuilder =>
            {
                temporalBuilder.UseHistoryTable("ProductsHistory");
                temporalBuilder.HasPeriodStart("ValidFromUtc");
                temporalBuilder.HasPeriodEnd("ValidToUtc");
            });
        });

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200);

        builder.Property(product => product.Price)
            .HasPrecision(18, 2);

        builder.Property(product => product.PrimaryImageUrl)
            .HasMaxLength(1000);

        builder.Property(product => product.VersionNumber)
            .HasDefaultValue(1);

        builder.Property(product => product.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(product => product.DeletedAtUtc == null);

        builder.HasIndex(product => product.CategoryId);
        builder.HasIndex(product => product.Name);
        builder.HasIndex(product => new { product.CategoryId, product.Name });
        builder.HasIndex(product => new { product.Price, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");
        builder.HasIndex(product => new { product.CategoryId, product.Price, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");
        builder.HasIndex(product => new { product.UpdatedAtUtc, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");
        builder.HasIndex(product => new { product.CategoryId, product.UpdatedAtUtc, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");
        builder.HasIndex(product => new { product.InventoryOnHand, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");
        builder.HasIndex(product => new { product.CategoryId, product.InventoryOnHand, product.Id })
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0");
            table.HasCheckConstraint("CK_Products_InventoryOnHand_NonNegative", "[InventoryOnHand] >= 0");
            table.HasCheckConstraint("CK_Products_VersionNumber_Positive", "[VersionNumber] >= 1");
        });
    }
}
