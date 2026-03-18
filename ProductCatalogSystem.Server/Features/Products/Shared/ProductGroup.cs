using FastEndpoints;

namespace ProductCatalogSystem.Server.Features.Products.Shared;

public sealed class ProductGroup : Group
{
    public ProductGroup()
    {
        Configure(
            "products",
            endpoint =>
            {
                endpoint.AllowAnonymous();
                endpoint.Description(description => description.WithTags("Products"));
            });
    }
}
