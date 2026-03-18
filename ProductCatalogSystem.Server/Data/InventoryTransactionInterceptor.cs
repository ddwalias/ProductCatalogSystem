using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProductCatalogSystem.Server.Data;

public sealed class InventoryTransactionInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyInventoryTransactions(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyInventoryTransactions(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ClearInventoryAudit(eventData.Context);
        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearInventoryAudit(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ClearInventoryAudit(eventData.Context);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ClearInventoryAudit(eventData.Context);
        return Task.CompletedTask;
    }

    private static void ApplyInventoryTransactions(DbContext? context)
    {
        if (context is CatalogDbContext catalogDbContext)
        {
            catalogDbContext.ApplyInventoryTransactions();
        }
    }

    private static void ClearInventoryAudit(DbContext? context)
    {
        if (context is CatalogDbContext catalogDbContext)
        {
            catalogDbContext.ClearInventoryAudit();
        }
    }
}
