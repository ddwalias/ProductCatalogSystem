using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCatalogSystem.Server.Features.Products.Search;

namespace ProductCatalogSystem.Server.Data;

public sealed class ProductSearchMessageInterceptor(
    IProductSearchMessagePublisher productSearchMessagePublisher) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureProductSearchChanges(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureProductSearchChanges(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishPendingProductSearchMessages(eventData.Context);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishPendingProductSearchMessagesAsync(eventData.Context, cancellationToken);
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ClearPendingProductSearchChanges(eventData.Context);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ClearPendingProductSearchChanges(eventData.Context);
        return Task.CompletedTask;
    }

    private static void CaptureProductSearchChanges(DbContext? context)
    {
        if (context is CatalogDbContext catalogDbContext)
        {
            catalogDbContext.CaptureProductSearchChanges();
        }
    }

    private void PublishPendingProductSearchMessages(DbContext? context)
    {
        if (context is CatalogDbContext catalogDbContext)
        {
            catalogDbContext.PublishPendingProductSearchMessages(productSearchMessagePublisher);
        }
    }

    private Task PublishPendingProductSearchMessagesAsync(
        DbContext? context,
        CancellationToken cancellationToken)
    {
        return context is CatalogDbContext catalogDbContext
            ? catalogDbContext.PublishPendingProductSearchMessagesAsync(
                productSearchMessagePublisher,
                cancellationToken)
            : Task.CompletedTask;
    }

    private static void ClearPendingProductSearchChanges(DbContext? context)
    {
        if (context is CatalogDbContext catalogDbContext)
        {
            catalogDbContext.ClearPendingProductSearchChanges();
        }
    }
}
