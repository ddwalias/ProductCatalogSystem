using FastEndpoints;

namespace ProductCatalogSystem.Server.Features.Categories.Shared;

public sealed class CategoryGroup : Group
{
    public CategoryGroup()
    {
        Configure(
            "categories",
            endpoint =>
            {
                endpoint.AllowAnonymous();
                endpoint.Description(description => description.WithTags("Categories"));
            });
    }
}
