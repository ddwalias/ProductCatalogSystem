using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Features.Categories.Shared;

public sealed record CategorySummaryDto(
    long Id,
    string Name,
    CategoryStatus Status,
    int DisplayOrder);

public sealed record CategoryTreeItemDto(
    long Id,
    long? ParentCategoryId,
    string Name,
    string? Description,
    CategoryStatus Status,
    int DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string RowVersion,
    List<CategoryTreeItemDto> Children);
