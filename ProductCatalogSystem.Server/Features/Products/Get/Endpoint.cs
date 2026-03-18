using FastEndpoints;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Get;

public sealed class Endpoint(IProductReader productReader)
    : CatalogEndpointWithoutRequest<ProductDetailDto>
{
    public override void Configure()
    {
        Get("{id:long}");
        Group<ProductGroup>();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id = Route<long>("id");
        var product = await productReader.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            await HttpContext.WriteProblemAsync(
                StatusCodes.Status404NotFound,
                "Resource not found",
                $"Product {id} was not found.",
                cancellationToken);
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(product, cancellationToken);
    }
}
