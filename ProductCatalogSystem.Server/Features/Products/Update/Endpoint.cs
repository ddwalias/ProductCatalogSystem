using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Update;

public sealed class Endpoint(IProductWriter productWriter)
    : CatalogEndpoint<UpdateProductRequest, ProductDetailDto>
{
    public override void Configure()
    {
        Put("{id:long}");
        Group<ProductGroup>();
        DontThrowIfValidationFails();
    }

    public override async Task HandleAsync(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        if (await TryHandleValidationFailuresAsync(cancellationToken))
        {
            return;
        }

        var result = await productWriter.UpdateAsync(Route<long>("id"), request, cancellationToken);
        if (await TryHandleServiceResultAsync(result, cancellationToken))
        {
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(result.Value, cancellationToken);
    }
}
