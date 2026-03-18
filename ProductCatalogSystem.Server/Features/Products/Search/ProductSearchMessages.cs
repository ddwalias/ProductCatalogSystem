namespace ProductCatalogSystem.Server.Features.Products.Search;

public sealed class ProductSearchUpsertRequested
{
    public ProductSearchUpsertRequested()
    {
    }

    public ProductSearchUpsertRequested(long productId)
    {
        ProductId = productId;
    }

    public long ProductId { get; set; }
}

public sealed class ProductSearchDeleteRequested
{
    public ProductSearchDeleteRequested()
    {
    }

    public ProductSearchDeleteRequested(long productId)
    {
        ProductId = productId;
    }

    public long ProductId { get; set; }
}
