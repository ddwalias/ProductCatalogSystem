using FastEndpoints;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Categories.Shared;

namespace ProductCatalogSystem.Server.Features.Categories.Create;

public sealed class Endpoint(ICategoryWriter categoryWriter)
    : CatalogEndpoint<CreateCategoryRequest, CategoryTreeItemDto>
{
    public override void Configure()
    {
        Post(string.Empty);
        Group<CategoryGroup>();
        DontThrowIfValidationFails();
    }

    public override async Task HandleAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (await TryHandleValidationFailuresAsync(cancellationToken))
        {
            return;
        }

        var result = await categoryWriter.CreateAsync(request, cancellationToken);
        if (await TryHandleServiceResultAsync(result, cancellationToken))
        {
            return;
        }

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        HttpContext.Response.Headers.Location = "/api/categories";
        await HttpContext.Response.WriteAsJsonAsync(result.Value, cancellationToken);
    }
}
