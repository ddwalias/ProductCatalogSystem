using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Categories.Create;
using ProductCatalogSystem.Server.Features.Categories.Shared;
using ProductCatalogSystem.Server.Features.Categories.Update;

namespace ProductCatalogSystem.Server.Tests;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task GetTreeAsync_ShouldReturnCategoriesInDisplayOrder()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category
            {
                Id = 1,
                Name = "Second",
                DisplayOrder = 20,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [1]
            },
            new Category
            {
                Id = 2,
                Name = "First",
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [2]
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCategoryReader(dbContext);

        var result = await service.GetTreeAsync(CancellationToken.None);

        result.Select(category => category.Name).Should().ContainInOrder("First", "Second");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateDisplayOrderWithinSiblings()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category
            {
                Id = 1,
                Name = "Root",
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [1]
            },
            new Category
            {
                Id = 2,
                Name = "Existing Child",
                ParentCategoryId = 1,
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [2]
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCategoryWriter(dbContext);

        var result = await service.CreateAsync(
            new CreateCategoryRequest
            {
                Name = "Another Child",
                ParentCategoryId = 1,
                DisplayOrder = 10,
                Status = CategoryStatus.Active
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.ValidationFailed);
        result.Errors.Should().ContainKey(nameof(CreateCategoryRequest.DisplayOrder));
    }

    [Fact]
    public async Task CreateAsync_ShouldPopulateTimestampsAutomatically()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateCategoryWriter(dbContext);

        var result = await service.CreateAsync(
            new CreateCategoryRequest
            {
                Name = "Root",
                DisplayOrder = 10,
                Status = CategoryStatus.Active
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value!.CreatedAtUtc.Should().NotBe(default);
        result.Value.UpdatedAtUtc.Should().Be(result.Value.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectCycles()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category
            {
                Id = 1,
                Name = "Root",
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [1]
            },
            new Category
            {
                Id = 2,
                Name = "Child",
                ParentCategoryId = 1,
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [2]
            },
            new Category
            {
                Id = 3,
                Name = "Grandchild",
                ParentCategoryId = 2,
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [3]
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCategoryWriter(dbContext);

        var result = await service.UpdateAsync(
            1,
            new UpdateCategoryRequest
            {
                Name = "Root",
                ParentCategoryId = 3,
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                RowVersion = RowVersionConverter.Encode([1])
            },
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.ValidationFailed);
        result.Errors.Should().ContainKey(nameof(CreateCategoryRequest.ParentCategoryId));
    }

    [Fact]
    public async Task GetTreeAsync_ShouldFailWhenHierarchyIsCorrupted()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Categories.AddRange(
            new Category
            {
                Id = 1,
                Name = "First",
                ParentCategoryId = 2,
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [1]
            },
            new Category
            {
                Id = 2,
                Name = "Second",
                ParentCategoryId = 1,
                DisplayOrder = 20,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [2]
            });
        await dbContext.SaveChangesAsync();

        var service = CreateCategoryReader(dbContext);

        var action = async () => await service.GetTreeAsync(CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The category hierarchy is corrupted.");
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .AddInterceptors(new EntityLifecycleInterceptor())
            .Options;

        return new CatalogDbContext(options);
    }

    private static CategoryReader CreateCategoryReader(CatalogDbContext dbContext)
    {
        return new CategoryReader(
            dbContext,
            NullLogger<CategoryReader>.Instance);
    }

    private static CategoryWriter CreateCategoryWriter(CatalogDbContext dbContext)
    {
        return new CategoryWriter(
            dbContext,
            NullLogger<CategoryWriter>.Instance);
    }
}
