using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Common.Behaviors;

/// <summary>
/// A behavior that processes Taxon regeneration work after a successful transaction.
/// This handles deduplicated work registered during the request via ITaxonWorkRegistry
/// AND processes any remaining "Dirty Flags" in the database.
/// </summary>
public class TaxonWorkBehavior<TRequest, TResponse>(
    IApplicationDbContext context,
    ITaxonWorkRegistry workRegistry,
    ITaxonHierarchyService hierarchyService,
    ITaxonRegenerationService regenerationService) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Only process background work after successful command execution
        if (request is IBaseRequest) 
        {
            await ProcessRegisteredWorkAsync(cancellationToken);
            await ProcessDatabaseDirtyFlagsAsync(cancellationToken);
        }

        return response;
    }

    /// <summary>
    /// Processes work specifically registered during this request (fastest sync).
    /// </summary>
    private async Task ProcessRegisteredWorkAsync(CancellationToken ct)
    {
        // 1. Process unique hierarchy rebuilds
        foreach (var taxonomyId in workRegistry.GetPendingHierarchyRebuilds())
        {
            await hierarchyService.RebuildHierarchyAsync(taxonomyId, ct);
        }

        // 2. Process unique product regenerations
        foreach (var taxonId in workRegistry.GetPendingProductRegenerations())
        {
            await regenerationService.RegenerateProductsForTaxonAsync(taxonId, ct);
        }
    }

    /// <summary>
    /// Processes any entities still marked as "Dirty" in the database (resilience check).
    /// </summary>
    private async Task ProcessDatabaseDirtyFlagsAsync(CancellationToken ct)
    {
        var dirtyTaxons = await context.Set<Taxon>()
            .Where(t => t.MarkedForRegenerateProducts)
            .ToListAsync(ct);

        if (!dirtyTaxons.Any()) return;

        foreach (var taxon in dirtyTaxons)
        {
            var result = await regenerationService.RegenerateProductsForTaxonAsync(taxon.Id, ct);
            if (!result.IsError)
            {
                taxon.MarkedForRegenerateProducts = false;
                context.Set<Taxon>().Update(taxon);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
