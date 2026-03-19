using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Products.Search;

namespace ProductCatalogSystem.Server.Data;

public sealed class CatalogDbContext : DbContext
{
    private readonly IProductSearchMessagePublisher productSearchMessagePublisher;
    private readonly ProductSearchChangeBuffer productSearchChanges = new();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : this(options, new NoOpProductSearchMessagePublisher())
    {
    }

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        IProductSearchMessagePublisher productSearchMessagePublisher)
        : base(options)
    {
        this.productSearchMessagePublisher = productSearchMessagePublisher;
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

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
        foreach (var entry in GetInventoryTrackedProductEntries())
        {
            AddInventoryTransaction(entry);
        }
    }

    internal void CaptureProductSearchChanges()
    {
        if (productSearchChanges.IsPublishing)
        {
            return;
        }

        productSearchChanges.ReplacePending(GetSearchTrackedProductEntries().Select(CreateProductSearchChange));
    }

    internal void PublishPendingProductSearchMessages()
        => PublishPendingProductSearchMessagesAsync(CancellationToken.None).GetAwaiter().GetResult();

    internal async Task PublishPendingProductSearchMessagesAsync(CancellationToken cancellationToken)
    {
        if (!productSearchChanges.TryBeginPublishing(out var changes))
        {
            return;
        }

        try
        {
            foreach (var change in changes)
            {
                await PublishProductSearchChangeAsync(change, cancellationToken);
            }

            if (ChangeTracker.HasChanges())
            {
                await SaveChangesAsync(cancellationToken);
            }
        }
        finally
        {
            productSearchChanges.EndPublishing();
        }
    }

    internal void ClearPendingProductSearchChanges()
    {
        productSearchChanges.Clear();
    }

    private IEnumerable<EntityEntry<Product>> GetInventoryTrackedProductEntries()
    {
        return ChangeTracker.Entries<Product>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);
    }

    private IEnumerable<EntityEntry<Product>> GetSearchTrackedProductEntries()
    {
        return ChangeTracker.Entries<Product>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }

    private void AddInventoryTransaction(EntityEntry<Product> entry)
    {
        if (entry.State == EntityState.Added)
        {
            AddInitialInventoryTransaction(entry.Entity);
            return;
        }

        AddInventoryUpdateTransaction(entry);
    }

    private void AddInitialInventoryTransaction(Product product)
    {
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
            CreatedAtUtc = entry.Entity.UpdatedAtUtc == default ? DateTime.UtcNow : entry.Entity.UpdatedAtUtc
        });
    }

    private static ProductSearchChange CreateProductSearchChange(EntityEntry<Product> entry)
    {
        return new ProductSearchChange(
            entry.Entity,
            entry.State == EntityState.Deleted ? ProductSearchChangeKind.Delete : ProductSearchChangeKind.Upsert);
    }

    private Task PublishProductSearchChangeAsync(
        ProductSearchChange change,
        CancellationToken cancellationToken)
    {
        return change.Kind == ProductSearchChangeKind.Delete
            ? productSearchMessagePublisher.PublishDeleteAsync(change.Product.Id, cancellationToken)
            : productSearchMessagePublisher.PublishUpsertAsync(change.Product.Id, cancellationToken);
    }

    private sealed record ProductSearchChange(Product Product, ProductSearchChangeKind Kind);

    private enum ProductSearchChangeKind
    {
        Upsert,
        Delete
    }

    private sealed class ProductSearchChangeBuffer
    {
        private readonly List<ProductSearchChange> pending = [];

        public bool IsPublishing { get; private set; }

        public void ReplacePending(IEnumerable<ProductSearchChange> changes)
        {
            pending.Clear();
            pending.AddRange(changes);
        }

        public bool TryBeginPublishing(out ProductSearchChange[] changes)
        {
            if (IsPublishing || pending.Count == 0)
            {
                changes = [];
                return false;
            }

            changes = pending.ToArray();
            pending.Clear();
            IsPublishing = true;
            return true;
        }

        public void EndPublishing()
        {
            IsPublishing = false;
        }

        public void Clear()
        {
            pending.Clear();
            IsPublishing = false;
        }
    }
}
