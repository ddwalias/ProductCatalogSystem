using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;

namespace ProductCatalogSystem.Server.Features.Categories.Shared;

public interface ICategoryReader
{
    Task<IReadOnlyList<CategoryTreeItemDto>> GetTreeAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
}

public sealed class CategoryReader(
    CatalogDbContext dbContext,
    ILogger<CategoryReader> logger) : ICategoryReader
{
    public async Task<IReadOnlyList<CategoryTreeItemDto>> GetTreeAsync(CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Id)
            .Select(category => new
            {
                category.Id,
                category.ParentCategoryId,
                category.Name,
                category.Description,
                category.Status,
                category.DisplayOrder,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
                category.RowVersion
            })
            .ToListAsync(cancellationToken);

        var lookup = categories.ToDictionary(
            category => category.Id,
            category => new CategoryTreeItemDto(
                category.Id,
                category.ParentCategoryId,
                category.Name,
                category.Description,
                category.Status,
                category.DisplayOrder,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
                RowVersionConverter.Encode(category.RowVersion),
                []));

        var roots = new List<CategoryTreeItemDto>();

        foreach (var category in categories)
        {
            var dto = lookup[category.Id];

            if (category.ParentCategoryId is { } parentId && lookup.TryGetValue(parentId, out var parent))
            {
                parent.Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        var visited = new HashSet<long>();
        foreach (var root in roots)
        {
            VisitCategoryTree(root, visited);
        }

        if (visited.Count != categories.Count)
        {
            logger.LogError(
                "Category hierarchy corruption detected. Expected {ExpectedCount} reachable categories but found {ReachableCount}.",
                categories.Count,
                visited.Count);
            throw new InvalidOperationException("The category hierarchy is corrupted.");
        }

        return roots;
    }

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        return dbContext.Categories.AnyAsync(category => category.Id == id, cancellationToken);
    }

    private static void VisitCategoryTree(CategoryTreeItemDto category, ISet<long> visited)
    {
        if (!visited.Add(category.Id))
        {
            return;
        }

        foreach (var child in category.Children)
        {
            VisitCategoryTree(child, visited);
        }
    }
}
