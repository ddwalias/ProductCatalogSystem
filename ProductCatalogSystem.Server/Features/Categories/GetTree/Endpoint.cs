using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Categories.Shared;

namespace ProductCatalogSystem.Server.Features.Categories.GetTree;

public sealed class Endpoint(ICategoryReader categoryReader)
    : CatalogEndpointWithoutRequest<IReadOnlyList<CategoryTreeItemDto>>
{
    public override void Configure()
    {
        Get(string.Empty);
        Group<CategoryGroup>();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryReader.GetTreeAsync(cancellationToken);
        await HttpContext.Response.WriteAsJsonAsync(categories, cancellationToken);
    }
}
