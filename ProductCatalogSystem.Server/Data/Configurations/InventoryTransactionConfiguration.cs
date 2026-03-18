using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data.Configurations;

internal sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.ChangeType)
            .HasConversion<byte>();

        builder.Property(transaction => transaction.Reason)
            .HasMaxLength(250);

        builder.Property(transaction => transaction.ChangedBy)
            .HasMaxLength(100);

        builder.HasOne(transaction => transaction.Product)
            .WithMany(product => product.InventoryTransactions)
            .HasForeignKey(transaction => transaction.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
