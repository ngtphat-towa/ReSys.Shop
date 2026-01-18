using MediatR;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using Microsoft.Extensions.Logging;

namespace ReSys.Core.Common.Behaviors;

/// <summary>
/// Pipeline behavior that executes accumulated taxon work (hierarchy rebuilds, product regenerations)
/// after the request has been handled and domain events have been processed.
/// </summary>
public class TaxonWorkBehavior<TRequest, TResponse>(
    ITaxonWorkRegistry registry,
    ITaxonHierarchyService hierarchyService,
    ITaxonRegenerationService regenerationService,
    ILogger<TaxonWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var response = await next();

        var pendingRebuilds = registry.GetPendingHierarchyRebuilds();
        var pendingRegenerations = registry.GetPendingProductRegenerations();

        if (pendingRebuilds.Count > 0 || pendingRegenerations.Count > 0)
        {
            logger.LogInformation("[WORK] Processing deferred taxon work: {RebuildCount} rebuilds, {RegenCount} regenerations", 
                pendingRebuilds.Count, pendingRegenerations.Count);
        }

        foreach (var taxonomyId in pendingRebuilds)
        {
            logger.LogDebug("[WORK] Executing deferred HIERARCHY REBUILD for Taxonomy {TaxonomyId}", taxonomyId);
            await hierarchyService.RebuildHierarchyAsync(taxonomyId, cancellationToken);
        }

        foreach (var taxonId in pendingRegenerations)
        {
            logger.LogDebug("[WORK] Executing deferred PRODUCT REGENERATION for Taxon {TaxonId}", taxonId);
            await regenerationService.RegenerateProductsForTaxonAsync(taxonId, cancellationToken);
        }

        return response;
    }
}
