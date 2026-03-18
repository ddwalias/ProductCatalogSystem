using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", table =>
        {
            table.HasTrigger("TR_Categories_PreventCycles");
            table.HasCheckConstraint("CK_Categories_DisplayOrder_NonNegative", "[DisplayOrder] >= 0");
        });

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .HasMaxLength(200);

        builder.Property(category => category.Status)
            .HasConversion(
                status => status == CategoryStatus.Active,
                isActive => isActive ? CategoryStatus.Active : CategoryStatus.Inactive)
            .HasColumnType("bit");

        builder.Property(category => category.RowVersion)
            .IsRowVersion();

        builder.HasIndex(category => category.ParentCategoryId);
        builder.HasIndex(category => new { category.Status, category.ParentCategoryId });
        builder.HasIndex(category => new { category.ParentCategoryId, category.DisplayOrder })
            .IsUnique();
        builder.HasIndex(category => category.DisplayOrder)
            .IsUnique()
            .HasFilter("[ParentCategoryId] IS NULL");

        builder.HasOne(category => category.ParentCategory)
            .WithMany(category => category.Children)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
