using ProductCatalogSystem.Server.Features.Products.Create;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class UpdateProductRequest : CreateProductRequest
{
    public string RowVersion { get; set; } = string.Empty;
}
