using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Features.Categories.Create;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? ParentCategoryId { get; set; }

    public CategoryStatus Status { get; set; } = CategoryStatus.Active;

    public decimal DisplayOrder { get; set; }
}
