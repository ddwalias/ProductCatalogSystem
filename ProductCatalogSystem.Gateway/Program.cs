using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        [
            new RouteConfig
            {
                RouteId = "server-api",
                ClusterId = "server",
                Match = new RouteMatch { Path = "/api/{**catch-all}" }
            },
            new RouteConfig
            {
                RouteId = "frontend",
                ClusterId = "webfrontend",
                Order = 100,
                Match = new RouteMatch { Path = "/{**catch-all}" }
            }
        ],
        [
            BuildCluster("server", ResolveServiceAddress(builder.Configuration, "server", "https")
                ?? ResolveServiceAddress(builder.Configuration, "server", "http")),
            BuildCluster("webfrontend", ResolveServiceAddress(builder.Configuration, "webfrontend", "http")
                ?? ResolveServiceAddress(builder.Configuration, "server", "https")
                ?? ResolveServiceAddress(builder.Configuration, "server", "http"))
        ]);

var app = builder.Build();

app.MapReverseProxy();

app.Run();

static ClusterConfig BuildCluster(string clusterId, string? address)
{
    var resolvedAddress = EnsureTrailingSlash(address
        ?? throw new InvalidOperationException($"Missing endpoint configuration for '{clusterId}'."));

    return new ClusterConfig
    {
        ClusterId = clusterId,
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            [clusterId] = new() { Address = resolvedAddress }
        }
    };
}

static string? ResolveServiceAddress(IConfiguration configuration, string serviceName, string endpointName)
    => configuration[$"services:{serviceName}:{endpointName}:0"];

static string EnsureTrailingSlash(string address)
    => address.EndsWith("/", StringComparison.Ordinal) ? address : $"{address}/";
