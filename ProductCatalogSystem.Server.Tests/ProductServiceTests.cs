using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Products.Create;
using ProductCatalogSystem.Server.Features.Products.Search;
using ProductCatalogSystem.Server.Features.Products.Shared;
using ProductCatalogSystem.Server.Features.Products.Update;

namespace ProductCatalogSystem.Server.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldRejectInactiveCategoryAssignments()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Inactive",
            DisplayOrder = 10,
            Status = CategoryStatus.Inactive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = CreateProductWriter(dbContext);

        var result = await service.CreateAsync(
            new CreateProductRequest
            {
                Name = "Blocked Product",
                CategoryId = 1,
                Price = 12.5m,
                InventoryOnHand = 3
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.ValidationFailed);
        result.Errors.Should().ContainKey(nameof(CreateProductRequest.CategoryId));
    }

    [Fact]
    public async Task CreateAsync_ShouldWriteInitialInventoryTransactionThroughEfCore()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Active",
            DisplayOrder = 10,
            Status = CategoryStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = CreateProductWriter(dbContext);

        var result = await service.CreateAsync(
            new CreateProductRequest
            {
                Name = "Tracked Product",
                CategoryId = 1,
                Price = 25m,
                InventoryOnHand = 7
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value!.CreatedAtUtc.Should().NotBe(default);
        result.Value.UpdatedAtUtc.Should().Be(result.Value.CreatedAtUtc);

        var transaction = await dbContext.InventoryTransactions.SingleAsync();
        transaction.ChangeType.Should().Be(InventoryChangeType.InitialStock);
        transaction.BeforeQty.Should().Be(0);
        transaction.AfterQty.Should().Be(7);
        transaction.Delta.Should().Be(7);
        transaction.ProductId.Should().Be(result.Value!.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldWriteInventoryTransactionThroughEfCore()
    {
        await using var dbContext = CreateDbContext();
        var now = DateTime.UtcNow;
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Active",
            DisplayOrder = 10,
            Status = CategoryStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            RowVersion = [1]
        });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Tracked Product",
            CategoryId = 1,
            Price = 50m,
            InventoryOnHand = 5,
            VersionNumber = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = CreateProductWriter(dbContext);

        var result = await service.UpdateAsync(
            1,
            new UpdateProductRequest
            {
                Name = "Tracked Product",
                CategoryId = 1,
                Price = 50m,
                InventoryOnHand = 8,
                RowVersion = RowVersionConverter.Encode([1])
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);

        var transaction = await dbContext.InventoryTransactions
            .SingleAsync(current => current.ChangeType == InventoryChangeType.ProductUpdate);
        transaction.ChangeType.Should().Be(InventoryChangeType.ProductUpdate);
        transaction.BeforeQty.Should().Be(5);
        transaction.AfterQty.Should().Be(8);
        transaction.Delta.Should().Be(3);
        transaction.ProductId.Should().Be(1);

        (await dbContext.InventoryTransactions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProductAndPreserveLedger()
    {
        await using var dbContext = CreateDbContext();
        var now = DateTime.UtcNow;
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Active",
            DisplayOrder = 10,
            Status = CategoryStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            RowVersion = [1]
        });
        dbContext.Products.Add(new Product
        {
            Id = 1,
            Name = "Tracked Product",
            CategoryId = 1,
            Price = 50m,
            InventoryOnHand = 5,
            VersionNumber = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            RowVersion = [1]
        });
        await dbContext.SaveChangesAsync();

        var service = CreateProductWriter(dbContext);

        var deleted = await service.DeleteAsync(1, CancellationToken.None);

        deleted.Should().BeTrue();
        (await dbContext.Products.SingleOrDefaultAsync(product => product.Id == 1)).Should().BeNull();

        var deletedProduct = await dbContext.Products
            .IgnoreQueryFilters()
            .SingleAsync(product => product.Id == 1);

        deletedProduct.DeletedAtUtc.Should().NotBeNull();
        deletedProduct.UpdatedAtUtc.Should().Be(deletedProduct.DeletedAtUtc!.Value);
        deletedProduct.VersionNumber.Should().Be(2);
        (await dbContext.InventoryTransactions
            .IgnoreQueryFilters()
            .CountAsync()).Should().Be(1);
    }

    [Fact]
    public void ProductListQuery_ShouldNormalizeSortSettings()
    {
        var query = new ProductListQuery
        {
            SortBy = "UpdatedAt",
            SortDir = "DESC"
        };

        query.NormalizedSortBy.Should().Be("updatedat");
        query.SortDescending.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsAsync_ShouldApplyPriceRangeFilters()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Active",
            DisplayOrder = 10,
            Status = CategoryStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = [1]
        });
        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Budget Mouse",
                CategoryId = 1,
                Price = 25m,
                InventoryOnHand = 5,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Premium Keyboard",
                CategoryId = 1,
                Price = 180m,
                InventoryOnHand = 9,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Desk Lamp",
                CategoryId = 1,
                Price = 70m,
                InventoryOnHand = 3,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var service = CreateProductReader(dbContext);

        var result = await service.GetProductsAsync(
            new ProductListQuery
            {
                PriceFrom = 50m,
                PriceTo = 100m
            },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Name.Should().Be("Desk Lamp");
    }

    [Fact]
    public async Task GetAutocompleteAsync_ShouldReturnPrefixMatchesFirst()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.Add(new Category
        {
            Id = 1,
            Name = "Active",
            DisplayOrder = 10,
            Status = CategoryStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            RowVersion = [1]
        });
        dbContext.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Wireless Headphones",
                CategoryId = 1,
                Price = 120m,
                InventoryOnHand = 5,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Pro Wireless Mouse",
                CategoryId = 1,
                Price = 80m,
                InventoryOnHand = 4,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Cable Organizer",
                CategoryId = 1,
                Price = 20m,
                InventoryOnHand = 12,
                VersionNumber = 1,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var service = CreateProductReader(dbContext);

        var result = await service.GetAutocompleteAsync(
            new ProductAutocompleteQuery
            {
                Query = "Wire",
                Limit = 5
            },
            CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Wireless Headphones");
        result[1].Name.Should().Be("Pro Wireless Mouse");
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .AddInterceptors(
                new ProductSearchMessageInterceptor(),
                new EntityLifecycleInterceptor(),
                new InventoryTransactionInterceptor())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CatalogDbContext(options);
    }

    private static ProductReader CreateProductReader(CatalogDbContext dbContext)
    {
        return new ProductReader(
            dbContext,
            new NoOpProductSearchIndex(),
            NullLogger<ProductReader>.Instance);
    }

    private static ProductWriter CreateProductWriter(CatalogDbContext dbContext)
    {
        return new ProductWriter(
            dbContext,
            NullLogger<ProductWriter>.Instance,
            CreateProductReader(dbContext));
    }
}
