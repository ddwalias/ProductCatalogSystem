using ProductCatalogSystem.Server.Features.Categories.Shared;
using ProductCatalogSystem.Server.Features.Products.Search;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogFeatures(this IServiceCollection services, bool useElasticsearch, bool useMessageBus)
    {
        if (useElasticsearch)
        {
            services.AddScoped<IProductSearchIndex, ElasticsearchProductSearchIndex>();
        }
        else
        {
            services.AddScoped<IProductSearchIndex, NoOpProductSearchIndex>();
        }

        if (useMessageBus)
        {
            services.AddScoped<IProductSearchMessagePublisher, ProductSearchMessagePublisher>();
        }
        else
        {
            services.AddScoped<IProductSearchMessagePublisher, NoOpProductSearchMessagePublisher>();
        }

        services.AddScoped<IProductReader, ProductReader>();
        services.AddScoped<IProductWriter, ProductWriter>();
        services.AddScoped<ICategoryReader, CategoryReader>();
        services.AddScoped<ICategoryWriter, CategoryWriter>();

        return services;
    }
}
