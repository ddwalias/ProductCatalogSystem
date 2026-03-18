using FastEndpoints;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.List;

public sealed class Endpoint(IProductReader productReader)
    : CatalogEndpoint<ProductListQuery, PagedResult<ProductListItemDto>>
{
    public override void Configure()
    {
        Get(string.Empty);
        Group<ProductGroup>();
    }

    public override async Task HandleAsync(ProductListQuery request, CancellationToken cancellationToken)
    {
        var result = await productReader.GetProductsAsync(request, cancellationToken);
        await HttpContext.Response.WriteAsJsonAsync(result, cancellationToken);
    }
}
