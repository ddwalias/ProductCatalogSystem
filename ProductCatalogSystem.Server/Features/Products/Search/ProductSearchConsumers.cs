using MassTransit;

namespace ProductCatalogSystem.Server.Features.Products.Search;

public sealed class ProductSearchUpsertRequestedConsumer(
    IProductSearchIndex productSearchIndex,
    ILogger<ProductSearchUpsertRequestedConsumer> logger) : IConsumer<ProductSearchUpsertRequested>
{
    public async Task Consume(ConsumeContext<ProductSearchUpsertRequested> context)
    {
        if (!productSearchIndex.IsEnabled)
        {
            logger.LogInformation(
                "Skipping product search upsert for {ProductId} because Elasticsearch is disabled",
                context.Message.ProductId);
            return;
        }

        await productSearchIndex.UpsertAsync(context.Message.ProductId, context.CancellationToken);
    }
}

public sealed class ProductSearchDeleteRequestedConsumer(
    IProductSearchIndex productSearchIndex,
    ILogger<ProductSearchDeleteRequestedConsumer> logger) : IConsumer<ProductSearchDeleteRequested>
{
    public async Task Consume(ConsumeContext<ProductSearchDeleteRequested> context)
    {
        if (!productSearchIndex.IsEnabled)
        {
            logger.LogInformation(
                "Skipping product search delete for {ProductId} because Elasticsearch is disabled",
                context.Message.ProductId);
            return;
        }

        await productSearchIndex.DeleteAsync(context.Message.ProductId, context.CancellationToken);
    }
}

public sealed class ProductSearchUpsertRequestedConsumerDefinition
    : ConsumerDefinition<ProductSearchUpsertRequestedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductSearchUpsertRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseDelayedRedelivery(redelivery =>
            redelivery.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(15)));

        endpointConfigurator.UseKillSwitch(options => options
            .SetActivationThreshold(2)
            .SetTripThreshold(0.20)
            .SetRestartTimeout(m: 1));
    }
}

public sealed class ProductSearchDeleteRequestedConsumerDefinition
    : ConsumerDefinition<ProductSearchDeleteRequestedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductSearchDeleteRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseDelayedRedelivery(redelivery =>
            redelivery.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(15)));

        endpointConfigurator.UseKillSwitch(options => options
            .SetActivationThreshold(2)
            .SetTripThreshold(0.20)
            .SetRestartTimeout(m: 1));
    }
}
