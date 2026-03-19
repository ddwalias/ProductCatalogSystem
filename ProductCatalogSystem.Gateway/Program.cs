using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var enableHttpsRedirection = builder.Environment.IsProduction();

if (enableHttpsRedirection)
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
    });
}

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        [
            new RouteConfig
            {
                RouteId = "dashboard-otlp",
                ClusterId = "compose-dashboard-otlp",
                Order = -200,
                Match = new RouteMatch { Path = "/dashboard-otlp/{**catch-all}" },
                Transforms =
                [
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["PathRemovePrefix"] = "/dashboard-otlp"
                    }
                ]
            },
            new RouteConfig
            {
                RouteId = "server-health",
                ClusterId = "server",
                Order = -100,
                Match = new RouteMatch { Path = "/api/health" },
                Transforms =
                [
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["PathSet"] = "/health"
                    }
                ]
            },
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
            BuildCluster("compose-dashboard-otlp", "http://compose-dashboard:18890"),
            BuildCluster("server", ResolveServiceAddress(builder.Configuration, "server", "https")
                ?? ResolveServiceAddress(builder.Configuration, "server", "http")),
            BuildCluster("webfrontend", ResolveServiceAddress(builder.Configuration, "webfrontend", "http")
                ?? ResolveServiceAddress(builder.Configuration, "server", "https")
                ?? ResolveServiceAddress(builder.Configuration, "server", "http"))
        ]);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();

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
