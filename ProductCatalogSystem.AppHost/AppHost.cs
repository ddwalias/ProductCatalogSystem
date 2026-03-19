var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var sql = builder.AddSqlServer("sql")
    .PublishAsDockerComposeService((_, service) => service.Name = "sql");
var catalogDb = sql.AddDatabase("catalogdb");
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithEnvironment("xpack.ml.enabled", "false")
    .PublishAsDockerComposeService((_, service) => service.Name = "elasticsearch");

var server = builder.AddProject<Projects.ProductCatalogSystem_Server>("server")
    .WithReference(catalogDb)
    .WithReference(elasticsearch)
    .WaitFor(catalogDb)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health")
    .PublishAsDockerComposeService((_, service) => service.Name = "server");

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WaitFor(server);

if (builder.ExecutionContext.IsPublishMode)
{
    server.PublishWithContainerFiles(webfrontend, "./wwwroot");

    builder.AddProject<Projects.ProductCatalogSystem_Gateway>("gateway")
        .WithReference(server)
        .WithExternalHttpEndpoints()
        .WaitFor(server)
        .PublishAsDockerComposeService((_, service) => service.Name = "gateway");
}
else
{
    builder.AddProject<Projects.ProductCatalogSystem_Gateway>("gateway")
        .WithReference(server)
        .WithReference(webfrontend)
        .WithExternalHttpEndpoints()
        .WaitFor(server)
        .WaitFor(webfrontend);
}

builder.Build().Run();
