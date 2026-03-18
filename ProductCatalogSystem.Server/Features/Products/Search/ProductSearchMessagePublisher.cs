using MassTransit;

namespace ProductCatalogSystem.Server.Features.Products.Search;

public interface IProductSearchMessagePublisher
{
    Task PublishUpsertAsync(long productId, CancellationToken cancellationToken);

    Task PublishDeleteAsync(long productId, CancellationToken cancellationToken);
}

public sealed class ProductSearchMessagePublisher(IPublishEndpoint publishEndpoint) : IProductSearchMessagePublisher
{
    public Task PublishUpsertAsync(long productId, CancellationToken cancellationToken)
        => publishEndpoint.Publish(new ProductSearchUpsertRequested(productId), cancellationToken);

    public Task PublishDeleteAsync(long productId, CancellationToken cancellationToken)
        => publishEndpoint.Publish(new ProductSearchDeleteRequested(productId), cancellationToken);
}

public sealed class NoOpProductSearchMessagePublisher : IProductSearchMessagePublisher
{
    public Task PublishUpsertAsync(long productId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task PublishDeleteAsync(long productId, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
