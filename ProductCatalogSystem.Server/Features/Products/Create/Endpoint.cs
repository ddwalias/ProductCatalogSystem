using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Create;

public sealed class Endpoint(IProductWriter productWriter)
    : CatalogEndpoint<CreateProductRequest, ProductDetailDto>
{
    public override void Configure()
    {
        Post(string.Empty);
        Group<ProductGroup>();
        DontThrowIfValidationFails();
    }

    public override async Task HandleAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (await TryHandleValidationFailuresAsync(cancellationToken))
        {
            return;
        }

        var result = await productWriter.CreateAsync(request, cancellationToken);
        if (await TryHandleServiceResultAsync(result, cancellationToken))
        {
            return;
        }

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        HttpContext.Response.Headers.Location = $"/api/products/{result.Value!.Id}";
        await HttpContext.Response.WriteAsJsonAsync(result.Value, cancellationToken);
    }
}
