using ProductCatalogSystem.Server.Features.Categories.Create;

namespace ProductCatalogSystem.Server.Features.Categories.Update;

public sealed class UpdateCategoryRequest : CreateCategoryRequest
{
    public string RowVersion { get; set; } = string.Empty;
}
