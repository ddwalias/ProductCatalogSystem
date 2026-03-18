using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Categories.Shared;

namespace ProductCatalogSystem.Server.Features.Categories.Update;

public sealed class Endpoint(ICategoryWriter categoryWriter)
    : CatalogEndpoint<UpdateCategoryRequest, CategoryTreeItemDto>
{
    public override void Configure()
    {
        Put("{id:long}");
        Group<CategoryGroup>();
        DontThrowIfValidationFails();
    }

    public override async Task HandleAsync(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (await TryHandleValidationFailuresAsync(cancellationToken))
        {
            return;
        }

        var result = await categoryWriter.UpdateAsync(Route<long>("id"), request, cancellationToken);
        if (await TryHandleServiceResultAsync(result, cancellationToken))
        {
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(result.Value, cancellationToken);
    }
}
