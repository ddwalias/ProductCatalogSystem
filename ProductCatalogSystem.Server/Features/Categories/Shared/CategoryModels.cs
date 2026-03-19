using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Features.Categories.Shared;

public sealed record CategoryTreeItemDto(
    long Id,
    long? ParentCategoryId,
    string Name,
    string? Description,
    CategoryStatus Status,
    decimal DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string RowVersion,
    List<CategoryTreeItemDto> Children);
