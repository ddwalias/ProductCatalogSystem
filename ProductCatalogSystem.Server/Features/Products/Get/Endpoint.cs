using System.Collections.Generic;
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
        if (!TryGetRequestedVersion(out var version, out var errors))
        {
            await HttpContext.WriteValidationProblemAsync(errors, cancellationToken);
            return;
        }

        var product = await productReader.GetByIdAsync(id, version, cancellationToken);

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

    private bool TryGetRequestedVersion(
        out int? version,
        out Dictionary<string, string[]> errors)
    {
        errors = [];
        version = null;

        if (!HttpContext.Request.Query.TryGetValue("version", out var rawVersion))
        {
            return true;
        }

        if (rawVersion.Count != 1 ||
            string.IsNullOrWhiteSpace(rawVersion[0]) ||
            !int.TryParse(rawVersion[0], out var parsedVersion) ||
            parsedVersion < 1)
        {
            errors["Version"] = ["Version must be a positive integer."];
            return false;
        }

        version = parsedVersion;
        return true;
    }
}
