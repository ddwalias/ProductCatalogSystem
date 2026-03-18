using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Tests;

public sealed class ApiEndpointTests
{
    [Fact]
    public async Task GetCategories_ShouldUseFastEndpointsRouteAndReturnJson()
    {
        await using var factory = new ApiWebApplicationFactory();
        await factory.SeedAsync(dbContext =>
        {
            dbContext.Categories.Add(new Category
            {
                Id = 7,
                Name = "Accessories",
                DisplayOrder = 10,
                Status = CategoryStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                RowVersion = [1]
            });

            return dbContext.SaveChangesAsync();
        });

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        payload.Should().NotBeNull();
        payload!.Should().ContainSingle(category => category.Id == 7 && category.Name == "Accessories");
    }

    [Fact]
    public async Task SearchProducts_ShouldPreserveValidationProblemShape()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products/search?query=%20%20%20");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        payload.Should().NotBeNull();
        payload!.Errors.Should().ContainKey("Query");
        payload.Errors["Query"].Should().Contain("Search query is required.");
    }

    [Fact]
    public async Task CreateCategory_ShouldRejectOutOfRangeStatus()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/categories", new
        {
            name = "Invalid Status Category",
            displayOrder = 10,
            status = 9
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        payload.Should().NotBeNull();
        payload!.Errors.Should().ContainKey("Status");
    }

    private sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SkipCatalogInitialization"] = "true"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<CatalogDbContext>();
                services.RemoveAll<DbContextOptions<CatalogDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<CatalogDbContext>>();

                services.AddDbContextPool<CatalogDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                    options.AddInterceptors(new EntityLifecycleInterceptor());
                });
            });
        }

        public async Task SeedAsync(Func<CatalogDbContext, Task> seed)
        {
            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await seed(dbContext);
        }
    }

    private sealed record CategoryResponse(long Id, string Name);

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}
