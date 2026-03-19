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

if (builder.ExecutionContext.IsPublishMode)
{
    var webfrontend = builder.AddDockerfile("webfrontend", "../frontend")
        .WithHttpEndpoint(targetPort: 3000, name: "http")
        .WaitFor(server)
        .PublishAsDockerComposeService((_, service) => service.Name = "webfrontend");

    builder.AddProject<Projects.ProductCatalogSystem_Gateway>("gateway")
        .WithReference(server)
        .WithReference(webfrontend.GetEndpoint("http"))
        .WithExternalHttpEndpoints()
        .WaitFor(server)
        .WaitFor(webfrontend)
        .PublishAsDockerComposeService((_, service) => service.Name = "gateway");
}
else
{
    var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
        .WaitFor(server);

    builder.AddProject<Projects.ProductCatalogSystem_Gateway>("gateway")
        .WithReference(server)
        .WithReference(webfrontend)
        .WithExternalHttpEndpoints()
        .WaitFor(server)
        .WaitFor(webfrontend);
}

builder.Build().Run();
