using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCatalogSystem.Server.Domain;

namespace ProductCatalogSystem.Server.Data;

public sealed class EntityLifecycleInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyLifecycle(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyLifecycle(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void ApplyLifecycle(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.UpdatedAtUtc = now;
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Property(entity => entity.CreatedAtUtc).IsModified = false;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletableEntity>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.DeletedAtUtc = now;

            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.UpdatedAtUtc = now;
                entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).IsModified = false;
            }
        }
    }
}
