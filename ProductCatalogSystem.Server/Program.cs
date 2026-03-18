using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Features;
using ProductCatalogSystem.Server.Features.Products.Search;

var builder = WebApplication.CreateBuilder(args);
var catalogConnectionString = builder.Configuration.GetConnectionString("catalogdb");
var hasElasticsearchConnection =
    !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("elasticsearch")) ||
    !string.IsNullOrWhiteSpace(builder.Configuration["Aspire:Elastic:Clients:Elasticsearch:Endpoint"]);
var hasMessageTransport = !string.IsNullOrWhiteSpace(catalogConnectionString);
var shouldSeedCatalog = builder.Environment.IsDevelopment() && !builder.Configuration.GetValue<bool>("SkipCatalogInitialization");

builder.AddServiceDefaults();
if (hasElasticsearchConnection)
{
    builder.AddElasticsearchClient("elasticsearch");
}

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(options =>
{
    options.DocumentSettings = settings =>
    {
        settings.DocumentName = "v1";
        settings.Title = "Product Catalog System API";
        settings.Version = "v1";
        settings.Description = "Interactive API explorer for the Product Catalog System.";
    };
});
builder.AddSqlServerDbContext<CatalogDbContext>(
    "catalogdb",
    configureDbContextOptions: options =>
    {
        options.AddInterceptors(
            new EntityLifecycleInterceptor(),
            new InventoryTransactionInterceptor());

        if (!shouldSeedCatalog)
        {
            return;
        }

        options.UseSeeding((context, _) => DbInitializer.SeedCatalog((CatalogDbContext)context));
        options.UseAsyncSeeding((context, _, cancellationToken) =>
            DbInitializer.SeedCatalogAsync((CatalogDbContext)context, cancellationToken));
    });

if (hasMessageTransport)
{
    builder.Services.AddOptions<SqlTransportOptions>()
        .Configure(options => options.ConnectionString = catalogConnectionString);
    builder.Services.AddSqlServerMigrationHostedService(options =>
    {
        options.CreateDatabase = false;
        options.CreateInfrastructure = true;
    });
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<ProductSearchUpsertRequestedConsumer>();
        x.AddConsumer<ProductSearchDeleteRequestedConsumer>();

        x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
        {
            o.UseSqlServer();
            o.UseBusOutbox();
        });

        x.AddConfigureEndpointsCallback((context, _, cfg) =>
        {
            cfg.UseEntityFrameworkOutbox<CatalogDbContext>(context);
        });

        x.UsingSqlServer((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("catalog-db");

builder.Services.AddCatalogFeatures(hasElasticsearchConnection, hasMessageTransport);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(
        settings =>
        {
            settings.Path = "/swagger/{documentName}/swagger.json";
        },
        uiSettings =>
        {
            uiSettings.Path = "/swagger";
            uiSettings.DocumentTitle = "Product Catalog System API Explorer";
            uiSettings.DocExpansion = "list";
        });
}
else
{
    app.UseHttpsRedirection();
}

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
});
app.MapDefaultEndpoints();
app.UseFileServer();

await app.Services.EnsureCatalogDatabaseReadyAsync();
await app.Services.InitializeProductSearchAsync();

app.Run();

public partial class Program;
