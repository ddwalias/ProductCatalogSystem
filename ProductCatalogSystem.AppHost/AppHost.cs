var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var catalogDb = sql.AddDatabase("catalogdb");
var elasticsearch = builder.AddElasticsearch("elasticsearch");

var server = builder.AddProject<Projects.ProductCatalogSystem_Server>("server")
    .WithReference(catalogDb)
    .WithReference(elasticsearch)
    .WaitFor(catalogDb)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health");

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WaitFor(server);

var gateway = builder.AddProject<Projects.ProductCatalogSystem_Gateway>("gateway")
    .WithReference(server)
    .WithReference(webfrontend)
    .WithExternalHttpEndpoints()
    .WaitFor(server)
    .WaitFor(webfrontend);

builder.Build().Run();
