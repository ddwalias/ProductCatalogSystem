using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data;

public sealed class CatalogDbContext : DbContext
{
    private InventoryAuditMetadata? inventoryAuditMetadata;

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    public void UseInventoryAudit(string? reason, string? changedBy)
    {
        inventoryAuditMetadata = new InventoryAuditMetadata(reason, changedBy);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    internal void ApplyInventoryTransactions()
    {
        var productEntries = ChangeTracker.Entries<Product>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToArray();

        if (productEntries.Length == 0)
        {
            return;
        }

        foreach (var entry in productEntries)
        {
            if (entry.State == EntityState.Added)
            {
                AddInitialInventoryTransaction(entry);
                continue;
            }

            AddInventoryUpdateTransaction(entry);
        }
    }

    internal void ClearInventoryAudit()
    {
        inventoryAuditMetadata = null;
    }

    private void AddInitialInventoryTransaction(EntityEntry<Product> entry)
    {
        var product = entry.Entity;
        if (product.InventoryOnHand <= 0)
        {
            return;
        }

        InventoryTransactions.Add(new InventoryTransaction
        {
            Product = product,
            ChangeType = InventoryChangeType.InitialStock,
            Delta = product.InventoryOnHand,
            BeforeQty = 0,
            AfterQty = product.InventoryOnHand,
            Reason = inventoryAuditMetadata?.Reason ?? "Initial stock",
            ChangedBy = inventoryAuditMetadata?.ChangedBy,
            CreatedAtUtc = product.CreatedAtUtc == default ? DateTime.UtcNow : product.CreatedAtUtc
        });
    }

    private void AddInventoryUpdateTransaction(EntityEntry<Product> entry)
    {
        var inventoryProperty = entry.Property(product => product.InventoryOnHand);
        if (!inventoryProperty.IsModified)
        {
            return;
        }

        var beforeQty = inventoryProperty.OriginalValue;
        var afterQty = inventoryProperty.CurrentValue;
        if (beforeQty == afterQty)
        {
            return;
        }

        InventoryTransactions.Add(new InventoryTransaction
        {
            Product = entry.Entity,
            ChangeType = InventoryChangeType.ProductUpdate,
            Delta = afterQty - beforeQty,
            BeforeQty = beforeQty,
            AfterQty = afterQty,
            Reason = inventoryAuditMetadata?.Reason ?? "Inventory updated with product edit",
            ChangedBy = inventoryAuditMetadata?.ChangedBy,
            CreatedAtUtc = entry.Entity.UpdatedAtUtc == default ? DateTime.UtcNow : entry.Entity.UpdatedAtUtc
        });
    }

    private sealed record InventoryAuditMetadata(string? Reason, string? ChangedBy);
}
