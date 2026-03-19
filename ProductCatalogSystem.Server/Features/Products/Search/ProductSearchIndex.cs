using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProductCatalogSystem.Server.Common;
using ProductCatalogSystem.Server.Data;
using ProductCatalogSystem.Server.Features.Products.Shared;

namespace ProductCatalogSystem.Server.Features.Products.Search;

public sealed record ProductSearchPage(
    IReadOnlyList<long> ProductIds,
    long TotalCount,
    string? NextCursor);

public sealed class ProductSearchIndexUnavailableException : Exception
{
    public ProductSearchIndexUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ProductSearchIndexAvailabilityState
{
    private readonly SemaphoreSlim unavailableSignal = new(0, 1);
    private int isAvailable;

    public bool IsAvailable => Volatile.Read(ref isAvailable) == 1;

    public void MarkAvailable()
    {
        Interlocked.Exchange(ref isAvailable, 1);
    }

    public void MarkUnavailable()
    {
        var wasAvailable = Interlocked.Exchange(ref isAvailable, 0) == 1;
        if (wasAvailable && unavailableSignal.CurrentCount == 0)
        {
            unavailableSignal.Release();
        }
    }

    public Task WaitUntilUnavailableAsync(CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            return Task.CompletedTask;
        }

        return unavailableSignal.WaitAsync(cancellationToken);
    }
}

public interface IProductSearchIndexRecovery
{
    Task RecoverAsync(CancellationToken cancellationToken);
}

public sealed class NoOpProductSearchIndexRecovery : IProductSearchIndexRecovery
{
    public Task RecoverAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class ProductSearchIndexRecoveryService(
    IServiceScopeFactory serviceScopeFactory,
    ProductSearchIndexAvailabilityState availabilityState,
    ILogger<ProductSearchIndexRecoveryService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await availabilityState.WaitUntilUnavailableAsync(stoppingToken);

            while (!availabilityState.IsAvailable && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = serviceScopeFactory.CreateAsyncScope();
                    var recovery = scope.ServiceProvider.GetRequiredService<IProductSearchIndexRecovery>();
                    await recovery.RecoverAsync(stoppingToken);
                    logger.LogInformation("Elasticsearch product search recovered");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        exception,
                        "Elasticsearch product search recovery failed. Will retry in {RetryDelaySeconds} seconds.",
                        RetryDelay.TotalSeconds);

                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }
        }
    }
}

public interface IProductSearchIndex
{
    bool IsEnabled { get; }

    Task<ProductSearchPage> SearchAsync(ProductListQuery query, PagingOptions paging, CancellationToken cancellationToken);

    Task<IReadOnlyList<long>> AutocompleteAsync(string query, int limit, CancellationToken cancellationToken);

    Task RebuildAsync(CancellationToken cancellationToken);

    Task UpsertAsync(long productId, CancellationToken cancellationToken);

    Task DeleteAsync(long productId, CancellationToken cancellationToken);
}

public sealed class NoOpProductSearchIndex : IProductSearchIndex
{
    public bool IsEnabled => false;

    public Task<IReadOnlyList<long>> AutocompleteAsync(string query, int limit, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<long>>([]);

    public Task DeleteAsync(long productId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task RebuildAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<ProductSearchPage> SearchAsync(ProductListQuery query, PagingOptions paging, CancellationToken cancellationToken)
        => Task.FromResult(new ProductSearchPage([], 0, null));

    public Task UpsertAsync(long productId, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class ElasticsearchProductSearchIndex(
    ElasticsearchClient client,
    CatalogDbContext dbContext,
    ProductSearchIndexAvailabilityState availabilityState,
    ILogger<ElasticsearchProductSearchIndex> logger) : IProductSearchIndex, IProductSearchIndexRecovery
{
    private const string IndexName = "catalog-products";
    private const string QueryAnalyzerName = "catalog_query";
    private const string PrefixAnalyzerName = "catalog_prefix";
    private const string InfixAnalyzerName = "catalog_infix";
    private const string FoldedNormalizerName = "catalog_folded";
    private const int ClusterHealthTimeoutSeconds = 60;
    private const int RecoveryBatchSize = 250;

    public bool IsEnabled => true;

    public async Task<ProductSearchPage> SearchAsync(ProductListQuery query, PagingOptions paging, CancellationToken cancellationToken)
    {
        var normalizedQuery = NormalizeQuery(query.Query);
        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return new ProductSearchPage([], 0, null);
        }

        ThrowIfUnavailable();

        try
        {
            var response = await client.SearchAsync<ProductSearchDocument>(
                search => search
                    .Indices(IndexName)
                    .Size(paging.PageSize + 1)
                    .SearchAfter(BuildSearchAfter(query, paging.Cursor))
                    .Query(descriptor => BuildSearchQuery(descriptor, normalizedQuery, query))
                    .Sort(
                        sort => sort.Score(score => score.Order(SortOrder.Desc)),
                        sort => sort.Field(field => field.Field(document => document.Id).Order(SortOrder.Asc))),
                cancellationToken);

            EnsureSuccess(response);

            var hits = response.Hits.ToList();
            var hasMore = hits.Count > paging.PageSize;
            var pageHits = hasMore
                ? hits.Take(paging.PageSize).ToList()
                : hits;

            var productIds = pageHits
                .Select(hit => hit.Source?.Id ?? ParseId(hit.Id))
                .Where(productId => productId > 0)
                .ToArray();

            var totalCount = response.HitsMetadata.Total?.Match(total => total?.Value ?? 0, value => value) ?? productIds.Length;
            var nextCursor = hasMore
                ? BuildNextCursor(query, pageHits[^1])
                : null;

            availabilityState.MarkAvailable();
            return new ProductSearchPage(productIds, totalCount, nextCursor);
        }
        catch (Exception exception)
        {
            throw MarkUnavailable("search", exception);
        }
    }

    public async Task<IReadOnlyList<long>> AutocompleteAsync(string query, int limit, CancellationToken cancellationToken)
    {
        var normalizedQuery = NormalizeQuery(query);
        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return [];
        }

        ThrowIfUnavailable();

        try
        {
            var response = await client.SearchAsync<ProductSearchDocument>(
                search => search
                    .Indices(IndexName)
                    .Size(limit)
                    .Query(descriptor => BuildAutocompleteQuery(descriptor, normalizedQuery))
                    .Sort(
                        sort => sort.Score(score => score.Order(SortOrder.Desc)),
                        sort => sort.Field(field => field.Field("name.keyword_folded").Order(SortOrder.Asc)),
                        sort => sort.Field(field => field.Field(document => document.Id).Order(SortOrder.Asc))),
                cancellationToken);

            EnsureSuccess(response);

            var productIds = response.Hits
                .Select(hit => hit.Source?.Id ?? ParseId(hit.Id))
                .Where(productId => productId > 0)
                .ToArray();

            availabilityState.MarkAvailable();
            return productIds;
        }
        catch (Exception exception)
        {
            throw MarkUnavailable("autocomplete", exception);
        }
    }

    public async Task RebuildAsync(CancellationToken cancellationToken)
    {
        await RecoverAsync(cancellationToken);
    }

    public async Task RecoverAsync(CancellationToken cancellationToken)
    {
        await WaitForClusterReadyAsync(cancellationToken);
        await RecreateIndexAsync(cancellationToken);

        var indexedCount = 0;
        long lastProductId = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id > lastProductId)
                .OrderBy(product => product.Id)
                .Take(RecoveryBatchSize)
                .Select(product => new ProductSearchDocument
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = (double)product.Price,
                    UpdatedAtUtc = product.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            if (products.Count == 0)
            {
                break;
            }

            var response = await client.IndexManyAsync(products, IndexName, cancellationToken);
            EnsureSuccess(response);

            if (response.Errors)
            {
                throw new InvalidOperationException("Elasticsearch bulk indexing failed during product search recovery.");
            }

            indexedCount += products.Count;
            lastProductId = products[^1].Id;
        }

        availabilityState.MarkAvailable();
        logger.LogInformation("Rebuilt Elasticsearch product index with {ProductCount} documents", indexedCount);
    }

    public async Task UpsertAsync(long productId, CancellationToken cancellationToken)
    {
        ThrowIfUnavailable();

        try
        {
            var document = await dbContext.Products
                .AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => new ProductSearchDocument
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = (double)product.Price,
                    UpdatedAtUtc = product.UpdatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (document is null)
            {
                await DeleteAsync(productId, cancellationToken);
                return;
            }

            var response = await client.IndexAsync(document, index => index.Index(IndexName).Id(document.Id), cancellationToken);
            EnsureSuccess(response);
            availabilityState.MarkAvailable();
        }
        catch (Exception exception)
        {
            throw MarkUnavailable("upsert", exception);
        }
    }

    public async Task DeleteAsync(long productId, CancellationToken cancellationToken)
    {
        ThrowIfUnavailable();

        try
        {
            var response = await client.DeleteAsync(new DeleteRequest(IndexName, productId), cancellationToken);
            if (!response.IsValidResponse &&
                response.ApiCallDetails.HttpStatusCode is not 404)
            {
                throw new InvalidOperationException($"Elasticsearch delete failed for product {productId}: {response.DebugInformation}");
            }

            availabilityState.MarkAvailable();
        }
        catch (Exception exception)
        {
            throw MarkUnavailable("delete", exception);
        }
    }

    private async Task RecreateIndexAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (existsResponse.Exists)
        {
            var deleteResponse = await client.Indices.DeleteAsync(IndexName, cancellationToken: cancellationToken);
            EnsureSuccess(deleteResponse);
        }

        await CreateIndexAsync(cancellationToken);
    }

    private void ThrowIfUnavailable()
    {
        if (!availabilityState.IsAvailable)
        {
            throw new ProductSearchIndexUnavailableException("Elasticsearch product search is currently unavailable.");
        }
    }

    private ProductSearchIndexUnavailableException MarkUnavailable(string operation, Exception exception)
    {
        availabilityState.MarkUnavailable();
        return new ProductSearchIndexUnavailableException(
            $"Elasticsearch product search {operation} failed.",
            exception);
    }

    private static QueryDescriptor<ProductSearchDocument> BuildSearchQuery(
        QueryDescriptor<ProductSearchDocument> descriptor,
        string normalizedQuery,
        ProductListQuery query)
    {
        var wildcardPattern = BuildWildcardPattern(normalizedQuery);
        var filters = BuildFilters(query);

        return descriptor.Bool(booleanQuery => booleanQuery
            .MinimumShouldMatch("1")
            .Should(
                should => should.MatchPhrase(match => match.Field(document => document.Name).Query(normalizedQuery).Boost(20)),
                should => should.MatchBoolPrefix(match => match
                    .Field("name.prefix")
                    .Query(normalizedQuery)
                    .Operator(Operator.And)
                    .MinimumShouldMatch("100%")
                    .Boost(16)),
                should => should.Match(match => match
                    .Field("name.infix")
                    .Query(normalizedQuery)
                    .Operator(Operator.And)
                    .MinimumShouldMatch("100%")
                    .Boost(12)),
                should => should.Match(match => match
                    .Field("description.infix")
                    .Query(normalizedQuery)
                    .Operator(Operator.And)
                    .MinimumShouldMatch("100%")
                    .Boost(5)),
                should => should.Match(match => match
                    .Field(document => document.Name)
                    .Query(normalizedQuery)
                    .Fuzziness("AUTO")
                    .FuzzyTranspositions(true)
                    .Boost(8)),
                should => should.Match(match => match
                    .Field(document => document.Description)
                    .Query(normalizedQuery)
                    .Fuzziness("AUTO")
                    .FuzzyTranspositions(true)
                    .Boost(2)),
                should => should.Prefix(prefix => prefix
                    .Field("name.keyword_folded")
                    .Value(normalizedQuery)
                    .Boost(14)),
                should => should.Wildcard(match => match
                    .Field("name.keyword_folded")
                    .Value(wildcardPattern)
                    .Boost(4)))
            .Filter(filters));
    }

    private static QueryDescriptor<ProductSearchDocument> BuildAutocompleteQuery(
        QueryDescriptor<ProductSearchDocument> descriptor,
        string normalizedQuery)
    {
        var wildcardPattern = BuildWildcardPattern(normalizedQuery);

        return descriptor.Bool(booleanQuery => booleanQuery
            .MinimumShouldMatch("1")
            .Should(
                should => should.Prefix(prefix => prefix.Field("name.keyword_folded").Value(normalizedQuery).Boost(24)),
                should => should.MatchBoolPrefix(match => match
                    .Field("name.prefix")
                    .Query(normalizedQuery)
                    .Operator(Operator.And)
                    .MinimumShouldMatch("100%")
                    .Boost(18)),
                should => should.Match(match => match
                    .Field("name.infix")
                    .Query(normalizedQuery)
                    .Operator(Operator.And)
                    .MinimumShouldMatch("100%")
                    .Boost(10)),
                should => should.Match(match => match
                    .Field(document => document.Name)
                    .Query(normalizedQuery)
                    .Fuzziness("AUTO")
                    .FuzzyTranspositions(true)
                    .Boost(6)),
                should => should.Wildcard(match => match.Field("name.keyword_folded").Value(wildcardPattern).Boost(3))));
    }

    private static List<Query> BuildFilters(ProductListQuery query)
    {
        var filters = new List<Query>();

        if (query.CategoryId is not null)
        {
            filters.Add(new QueryDescriptor<ProductSearchDocument>()
                .Term(term => term.Field(document => document.CategoryId).Value(query.CategoryId.Value)));
        }

        if (query.PriceFrom is not null)
        {
            filters.Add(new QueryDescriptor<ProductSearchDocument>()
                .Range(range => range.Number(numberRange =>
                    numberRange.Field(document => document.Price).Gte((double)query.PriceFrom.Value))));
        }

        if (query.PriceTo is not null)
        {
            filters.Add(new QueryDescriptor<ProductSearchDocument>()
                .Range(range => range.Number(numberRange =>
                    numberRange.Field(document => document.Price).Lte((double)query.PriceTo.Value))));
        }

        return filters;
    }

    private static string NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) ? character : ' ')
            .ToArray());

        return string.Join(
            ' ',
            normalized.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string BuildWildcardPattern(string normalizedQuery)
    {
        var terms = normalizedQuery
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return terms.Length == 0
            ? $"*{normalizedQuery}*"
            : $"*{string.Join('*', terms)}*";
    }

    private static long ParseId(Id? id)
    {
        return id is null
            ? 0
            : long.TryParse(id.ToString(), out var parsedId)
                ? parsedId
                : 0;
    }

    private static ICollection<FieldValue>? BuildSearchAfter(ProductListQuery query, string? cursor)
    {
        if (!ProductListCursor.TryDecodeSearchCursor(cursor, query, out var decodedCursor) || decodedCursor is null)
        {
            return null;
        }

        return
        [
            FieldValue.Double(decodedCursor.Score),
            FieldValue.Long(decodedCursor.Id)
        ];
    }

    private static string? BuildNextCursor(ProductListQuery query, Elastic.Clients.Elasticsearch.Core.Search.Hit<ProductSearchDocument> hit)
    {
        var productId = hit.Source?.Id ?? ParseId(hit.Id);
        if (productId <= 0 || hit.Score is null)
        {
            return null;
        }

        return ProductListCursor.EncodeSearchCursor(query, hit.Score.Value, productId);
    }

    private static void EnsureSuccess(ElasticsearchResponse response)
    {
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Elasticsearch request failed: {response.DebugInformation}");
        }
    }

    private async Task CreateIndexAsync(CancellationToken cancellationToken)
    {
        var indexDefinition = $$"""
            {
              "settings": {
                "index": {
                  "max_ngram_diff": 20,
                  "number_of_replicas": 0
                },
                "analysis": {
                  "normalizer": {
                    "{{FoldedNormalizerName}}": {
                      "type": "custom",
                      "filter": ["lowercase", "asciifolding"]
                    }
                  },
                  "filter": {
                    "{{PrefixAnalyzerName}}_filter": {
                      "type": "edge_ngram",
                      "min_gram": 2,
                      "max_gram": 20
                    },
                    "{{InfixAnalyzerName}}_filter": {
                      "type": "ngram",
                      "min_gram": 3,
                      "max_gram": 20
                    }
                  },
                  "analyzer": {
                    "{{QueryAnalyzerName}}": {
                      "tokenizer": "standard",
                      "filter": ["lowercase", "asciifolding"]
                    },
                    "{{PrefixAnalyzerName}}": {
                      "tokenizer": "standard",
                      "filter": ["lowercase", "asciifolding", "{{PrefixAnalyzerName}}_filter"]
                    },
                    "{{InfixAnalyzerName}}": {
                      "tokenizer": "standard",
                      "filter": ["lowercase", "asciifolding", "{{InfixAnalyzerName}}_filter"]
                    }
                  }
                }
              },
              "mappings": {
                "dynamic": "strict",
                "properties": {
                  "id": { "type": "long" },
                  "categoryId": { "type": "long" },
                  "price": { "type": "double" },
                  "updatedAtUtc": { "type": "date" },
                  "name": {
                    "type": "text",
                    "analyzer": "{{QueryAnalyzerName}}",
                    "fields": {
                      "prefix": {
                        "type": "text",
                        "analyzer": "{{PrefixAnalyzerName}}",
                        "search_analyzer": "{{QueryAnalyzerName}}"
                      },
                      "infix": {
                        "type": "text",
                        "analyzer": "{{InfixAnalyzerName}}",
                        "search_analyzer": "{{QueryAnalyzerName}}"
                      },
                      "keyword_folded": {
                        "type": "keyword",
                        "normalizer": "{{FoldedNormalizerName}}"
                      }
                    }
                  },
                  "description": {
                    "type": "text",
                    "analyzer": "{{QueryAnalyzerName}}",
                    "fields": {
                      "infix": {
                        "type": "text",
                        "analyzer": "{{InfixAnalyzerName}}",
                        "search_analyzer": "{{QueryAnalyzerName}}"
                      }
                    }
                  }
                }
              }
            }
            """;

        var endpoint = new EndpointPath(Elastic.Transport.HttpMethod.PUT, $"/{IndexName}");
        var response = await client.Transport.RequestAsync<StringResponse>(
            endpoint,
            PostData.String(indexDefinition),
            null,
            null,
            cancellationToken);

        EnsureSuccess(response);
        await WaitForIndexReadyAsync(cancellationToken);
    }

    private async Task WaitForClusterReadyAsync(CancellationToken cancellationToken)
    {
        var endpoint = new EndpointPath(
            Elastic.Transport.HttpMethod.GET,
            $"/_cluster/health?wait_for_status=yellow&wait_for_no_relocating_shards=true&timeout={ClusterHealthTimeoutSeconds}s");

        var response = await client.Transport.RequestAsync<StringResponse>(
            endpoint,
            null,
            null,
            null,
            cancellationToken);

        EnsureSuccess(response);

        if (response.Body.Contains("\"timed_out\":true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Elasticsearch cluster health wait timed out: {response.Body}");
        }
    }

    private async Task WaitForIndexReadyAsync(CancellationToken cancellationToken)
    {
        var endpoint = new EndpointPath(
            Elastic.Transport.HttpMethod.GET,
            $"/_cluster/health/{IndexName}?wait_for_status=yellow&wait_for_no_relocating_shards=true&timeout={ClusterHealthTimeoutSeconds}s");

        var response = await client.Transport.RequestAsync<StringResponse>(
            endpoint,
            null,
            null,
            null,
            cancellationToken);

        EnsureSuccess(response);

        if (response.Body.Contains("\"timed_out\":true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Elasticsearch index health wait timed out for '{IndexName}': {response.Body}");
        }
    }

    private static void EnsureSuccess(StringResponse response)
    {
        var statusCode = response.ApiCallDetails.HttpStatusCode ?? 0;
        if (statusCode is < 200 or >= 300)
        {
            throw new InvalidOperationException($"Elasticsearch request failed: {response.Body}");
        }
    }

    private sealed class ProductSearchDocument
    {
        public long Id { get; init; }

        public long CategoryId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public double Price { get; init; }

        public DateTime UpdatedAtUtc { get; init; }
    }
}
