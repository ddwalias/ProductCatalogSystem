using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;
using ProductCatalogSystem.Server.Features.Products.Search;

namespace ProductCatalogSystem.Server.Features.Products.Shared;

public interface IProductReader
{
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductListQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductAutocompleteItemDto>> GetAutocompleteAsync(
        ProductAutocompleteQuery query,
        CancellationToken cancellationToken);

    Task<ProductDetailDto?> GetByIdAsync(long id, int? version, CancellationToken cancellationToken);
}

public sealed class ProductReader(
    CatalogDbContext dbContext,
    IProductSearchIndex productSearchIndex,
    ILogger<ProductReader> logger) : IProductReader
{
    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductListQuery query, CancellationToken cancellationToken)
    {
        var paging = query.NormalizePaging();

        if (!string.IsNullOrWhiteSpace(query.Query) &&
            productSearchIndex.IsEnabled &&
            !query.HasExplicitSort)
        {
            try
            {
                return await GetProductsFromSearchIndexAsync(query, paging, cancellationToken);
            }
            catch (ProductSearchIndexUnavailableException)
            {
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Falling back to database product search for query '{Query}' because Elasticsearch is unavailable",
                    query.Query);
            }
        }

        var products = dbContext.Products.AsNoTracking();

        products = ApplySearch(products, query);
        products = ApplyFilters(products, query);

        var totalCount = await products.CountAsync(cancellationToken);
        var cursor = ProductListCursor.TryDecodeDatabaseCursor(paging.Cursor, query, out var decodedCursor)
            ? decodedCursor
            : null;

        products = ApplyCursor(products, cursor);
        products = ApplySorting(products, query);

        var items = await products
            .Take(paging.PageSize + 1)
            .Select(product => new ProductListItemDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.InventoryOnHand,
                product.PrimaryImageUrl,
                product.VersionNumber,
                product.CreatedAtUtc,
                product.UpdatedAtUtc,
                product.CategoryId,
                product.Category.Name))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > paging.PageSize;
        if (hasMore)
        {
            items = items.Take(paging.PageSize).ToList();
        }

        return new PagedResult<ProductListItemDto>(
            items,
            totalCount,
            paging.Cursor,
            hasMore ? ProductListCursor.EncodeDatabaseCursor(query, items[^1]) : null);
    }

    public async Task<IReadOnlyList<ProductAutocompleteItemDto>> GetAutocompleteAsync(
        ProductAutocompleteQuery query,
        CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Query.Trim();
        var normalizedSearchTerm = normalizedQuery.ToLowerInvariant();
        var normalizedLimit = Math.Clamp(query.Limit, 1, 20);

        if (productSearchIndex.IsEnabled)
        {
            try
            {
                var productIds = await productSearchIndex.AutocompleteAsync(normalizedQuery, normalizedLimit, cancellationToken);
                if (productIds.Count == 0)
                {
                    return [];
                }

                var productsById = await dbContext.Products
                    .AsNoTracking()
                    .Where(product => productIds.Contains(product.Id))
                    .Select(product => new ProductAutocompleteItemDto(
                        product.Id,
                        product.Name,
                        product.Category.Name,
                        product.PrimaryImageUrl))
                    .ToDictionaryAsync(product => product.Id, cancellationToken);

                return productIds
                    .Where(productsById.ContainsKey)
                    .Select(productId => productsById[productId])
                    .ToList();
            }
            catch (ProductSearchIndexUnavailableException)
            {
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Falling back to database autocomplete for query '{Query}' because Elasticsearch is unavailable",
                    normalizedQuery);
            }
        }

        var products = ApplyAutocompleteFallback(
            dbContext.Products.AsNoTracking(),
            normalizedSearchTerm);

        return await products
            .OrderBy(product => !product.Name.ToLower().StartsWith(normalizedSearchTerm))
            .ThenBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Take(normalizedLimit)
            .Select(product => new ProductAutocompleteItemDto(
                product.Id,
                product.Name,
                product.Category.Name,
                product.PrimaryImageUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailDto?> GetByIdAsync(long id, int? version, CancellationToken cancellationToken)
    {
        if (version is null)
        {
            return await ProjectProductDetail(
                    dbContext.Products
                        .AsNoTracking()
                        .Where(product => product.Id == id),
                    dbContext.Categories.AsNoTracking())
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await ProjectProductDetail(
                dbContext.Products
                    .TemporalAll()
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(product => product.Id == id && product.VersionNumber == version.Value)
                    .OrderByDescending(product => EF.Property<DateTime>(product, "ValidFromUtc")),
                dbContext.Categories
                    .IgnoreQueryFilters()
                    .AsNoTracking())
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ProductDetailDto> ProjectProductDetail(
        IQueryable<Product> products,
        IQueryable<Category> categories)
    {
        return
            from product in products
            join category in categories on product.CategoryId equals category.Id into categoryGroup
            from category in categoryGroup.DefaultIfEmpty()
            select new ProductDetailDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.InventoryOnHand,
                product.PrimaryImageUrl,
                JsonObjectSerializer.Deserialize(product.CustomAttributesJson),
                product.VersionNumber,
                product.CreatedAtUtc,
                product.UpdatedAtUtc,
                RowVersionConverter.Encode(product.RowVersion),
                product.CategoryId,
                category == null ? string.Empty : category.Name);
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> products, ProductListQuery query)
    {
        return (query.NormalizedSortBy, query.SortDescending) switch
        {
            ("price", false) => products.OrderBy(product => product.Price).ThenBy(product => product.Id),
            ("price", true) => products.OrderByDescending(product => product.Price).ThenBy(product => product.Id),
            ("updatedat", false) => products.OrderBy(product => product.UpdatedAtUtc).ThenBy(product => product.Id),
            ("updatedat", true) => products.OrderByDescending(product => product.UpdatedAtUtc).ThenBy(product => product.Id),
            ("inventory", false) => products.OrderBy(product => product.InventoryOnHand).ThenBy(product => product.Id),
            ("inventory", true) => products.OrderByDescending(product => product.InventoryOnHand).ThenBy(product => product.Id),
            ("name", true) => products.OrderByDescending(product => product.Name).ThenBy(product => product.Id),
            _ => products.OrderBy(product => product.Name).ThenBy(product => product.Id)
        };
    }

    private static IQueryable<Product> ApplySearch(IQueryable<Product> products, ProductListQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return products;
        }

        var terms = query.Query
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var term in terms)
        {
            var currentTerm = term.ToLowerInvariant();
            products = products.Where(product =>
                product.Name.ToLower().Contains(currentTerm) ||
                (product.Description != null && product.Description.ToLower().Contains(currentTerm)));
        }

        return products;
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> products, ProductListQuery query)
    {
        if (query.CategoryId is not null)
        {
            products = products.Where(product => product.CategoryId == query.CategoryId.Value);
        }

        if (query.PriceFrom is not null)
        {
            products = products.Where(product => product.Price >= query.PriceFrom.Value);
        }

        if (query.PriceTo is not null)
        {
            products = products.Where(product => product.Price <= query.PriceTo.Value);
        }

        return products;
    }

    private static IQueryable<Product> ApplyAutocompleteFallback(IQueryable<Product> products, string query)
    {
        return products.Where(product =>
            product.Name.ToLower().StartsWith(query) ||
            product.Name.ToLower().Contains(query));
    }

    private async Task<PagedResult<ProductListItemDto>> GetProductsFromSearchIndexAsync(
        ProductListQuery query,
        PagingOptions paging,
        CancellationToken cancellationToken)
    {
        var searchResult = await productSearchIndex.SearchAsync(query, paging, cancellationToken);
        if (searchResult.ProductIds.Count == 0)
        {
            return new PagedResult<ProductListItemDto>(
                [],
                0,
                paging.Cursor,
                null);
        }

        var itemsById = await dbContext.Products
            .AsNoTracking()
            .Where(product => searchResult.ProductIds.Contains(product.Id))
            .Select(product => new ProductListItemDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.InventoryOnHand,
                product.PrimaryImageUrl,
                product.VersionNumber,
                product.CreatedAtUtc,
                product.UpdatedAtUtc,
                product.CategoryId,
                product.Category.Name))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var orderedItems = searchResult.ProductIds
            .Where(itemsById.ContainsKey)
            .Select(productId => itemsById[productId])
            .ToList();

        var totalCount = (int)Math.Min(searchResult.TotalCount, int.MaxValue);
        return new PagedResult<ProductListItemDto>(
            orderedItems,
            totalCount,
            paging.Cursor,
            searchResult.NextCursor);
    }

    private static IQueryable<Product> ApplyCursor(
        IQueryable<Product> products,
        ProductListCursor.DatabaseCursor? cursor)
    {
        if (cursor is null)
        {
            return products;
        }

        return (cursor.SortBy, cursor.SortDescending) switch
        {
            ("price", false) when cursor.Price is not null =>
                products.Where(product => product.Price > cursor.Price.Value ||
                    (product.Price == cursor.Price.Value && product.Id > cursor.Id)),
            ("price", true) when cursor.Price is not null =>
                products.Where(product => product.Price < cursor.Price.Value ||
                    (product.Price == cursor.Price.Value && product.Id > cursor.Id)),
            ("updatedat", false) when cursor.UpdatedAtUtc is not null =>
                products.Where(product => product.UpdatedAtUtc > cursor.UpdatedAtUtc.Value ||
                    (product.UpdatedAtUtc == cursor.UpdatedAtUtc.Value && product.Id > cursor.Id)),
            ("updatedat", true) when cursor.UpdatedAtUtc is not null =>
                products.Where(product => product.UpdatedAtUtc < cursor.UpdatedAtUtc.Value ||
                    (product.UpdatedAtUtc == cursor.UpdatedAtUtc.Value && product.Id > cursor.Id)),
            ("inventory", false) when cursor.InventoryOnHand is not null =>
                products.Where(product => product.InventoryOnHand > cursor.InventoryOnHand.Value ||
                    (product.InventoryOnHand == cursor.InventoryOnHand.Value && product.Id > cursor.Id)),
            ("inventory", true) when cursor.InventoryOnHand is not null =>
                products.Where(product => product.InventoryOnHand < cursor.InventoryOnHand.Value ||
                    (product.InventoryOnHand == cursor.InventoryOnHand.Value && product.Id > cursor.Id)),
            ("name", true) when cursor.Name is not null =>
                products.Where(product => string.Compare(product.Name, cursor.Name) < 0 ||
                    (product.Name == cursor.Name && product.Id > cursor.Id)),
            _ when cursor.Name is not null =>
                products.Where(product => string.Compare(product.Name, cursor.Name) > 0 ||
                    (product.Name == cursor.Name && product.Id > cursor.Id)),
            _ => products
        };
    }
}
