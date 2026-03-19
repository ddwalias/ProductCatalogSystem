using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogSystem.Server.Common;

namespace ProductCatalogSystem.Server.Features.Products.Shared;

public sealed class ProductListQuery
{
    public int PageSize { get; set; } = PagingOptions.DefaultPageSize;

    public string? Cursor { get; set; }

    [FromQuery(Name = "query")]
    public string? Query { get; set; }

    public long? CategoryId { get; set; }

    public decimal? PriceFrom { get; set; }

    public decimal? PriceTo { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public PagingOptions NormalizePaging() => PagingOptions.Normalize(Cursor, PageSize);

    public string NormalizedSortBy => string.IsNullOrWhiteSpace(SortBy) ? "name" : SortBy.Trim().ToLowerInvariant();

    public bool SortDescending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);

    public bool HasExplicitSort =>
        !string.IsNullOrWhiteSpace(SortBy) ||
        !string.IsNullOrWhiteSpace(SortDir);
}

public sealed class ProductAutocompleteQuery
{
    [FromQuery(Name = "query")]
    public string Query { get; set; } = string.Empty;

    public int Limit { get; set; } = 8;
}

public sealed record ProductListItemDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    int InventoryOnHand,
    string? PrimaryImageUrl,
    int VersionNumber,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    long CategoryId,
    string CategoryName);

public sealed record ProductDetailDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    int InventoryOnHand,
    string? PrimaryImageUrl,
    JsonObject? CustomAttributes,
    int VersionNumber,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string RowVersion,
    long CategoryId,
    string CategoryName);

public sealed record ProductAutocompleteItemDto(
    long Id,
    string Name,
    string CategoryName,
    string? PrimaryImageUrl);
