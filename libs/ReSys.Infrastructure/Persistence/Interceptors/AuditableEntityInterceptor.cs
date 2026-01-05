using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using ReSys.Core.Domain.Abstractions.Concerns;
using ReSys.Core.Common.Security;

namespace ReSys.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.Id;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Auditable
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                    auditable.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    auditable.LastModifiedAt = now;
                    auditable.LastModifiedBy = userId;
                }
            }

            // Soft Deletable
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
            }

            // Versioned (Manual increment if not handled by DB/EF natively)
            if (entry.Entity is IVersioned versioned && entry.State == EntityState.Modified)
            {
                versioned.Version++;
            }
        }
    }
}

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}