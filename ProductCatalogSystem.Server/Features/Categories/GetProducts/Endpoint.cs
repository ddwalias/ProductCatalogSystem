using FastEndpoints;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Common.Endpoints;
using ProductCatalogSystem.Server.Features.Categories.Shared;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Categories.GetProducts;

public sealed class Endpoint(
    ICategoryReader categoryReader,
    IProductReader productReader) : CatalogEndpoint<ProductListQuery, PagedResult<ProductListItemDto>>
{
    public override void Configure()
    {
        Get("{id:long}/products");
        Group<CategoryGroup>();
    }

    public override async Task HandleAsync(ProductListQuery request, CancellationToken cancellationToken)
    {
        var categoryId = Route<long>("id");
        request.CategoryId = categoryId;
        var result = await productReader.GetProductsAsync(request, cancellationToken);

        if (result.TotalCount == 0 &&
            !await categoryReader.ExistsAsync(categoryId, cancellationToken))
        {
            await HttpContext.WriteProblemAsync(
                StatusCodes.Status404NotFound,
                "Resource not found",
                $"Category {categoryId} was not found.",
                cancellationToken);
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(result, cancellationToken);
    }
}
