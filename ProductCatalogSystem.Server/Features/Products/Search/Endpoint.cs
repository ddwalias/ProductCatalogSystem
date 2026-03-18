using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Search;

public sealed class Endpoint(IProductReader productReader)
    : CatalogEndpoint<ProductAutocompleteQuery, IReadOnlyList<ProductAutocompleteItemDto>>
{
    public override void Configure()
    {
        Get("search");
        Group<ProductGroup>();
        DontThrowIfValidationFails();
    }

    public override async Task HandleAsync(ProductAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (await TryHandleValidationFailuresAsync(cancellationToken))
        {
            return;
        }

        var result = await productReader.GetAutocompleteAsync(request, cancellationToken);
        await HttpContext.Response.WriteAsJsonAsync(result, cancellationToken);
    }
}
