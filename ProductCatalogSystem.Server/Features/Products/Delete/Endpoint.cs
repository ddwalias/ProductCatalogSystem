using FastEndpoints;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Delete;

public sealed class Endpoint(IProductWriter productWriter) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("{id:long}");
        Group<ProductGroup>();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id = Route<long>("id");
        var deleted = await productWriter.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            await HttpContext.WriteProblemAsync(
                StatusCodes.Status404NotFound,
                "Resource not found",
                $"Product {id} was not found.",
                cancellationToken);
            return;
        }

        HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
    }
}
