using MediatR;
using Microsoft.Extensions.Logging;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.RebuildTaxonomy;

public class TaxonHierarchyEventHandler(
    ITaxonWorkRegistry registry,
    ILogger<TaxonHierarchyEventHandler> logger) :
    INotificationHandler<TaxonEvents.TaxonCreated>,
    INotificationHandler<TaxonEvents.TaxonMoved>,
    INotificationHandler<TaxonEvents.TaxonDeleted>,
    INotificationHandler<TaxonEvents.TaxonUpdated>
{
    public async Task Handle(TaxonEvents.TaxonCreated notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred hierarchy rebuild for Taxon CREATED {TaxonId}", notification.Taxon.Id);
        registry.RegisterHierarchyRebuild(notification.Taxon.TaxonomyId);
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonEvents.TaxonMoved notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred hierarchy rebuild for Taxon MOVED {TaxonId}", notification.Taxon.Id);
        registry.RegisterHierarchyRebuild(notification.Taxon.TaxonomyId);
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonEvents.TaxonDeleted notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred hierarchy rebuild for Taxon DELETED {TaxonId}", notification.Taxon.Id);
        registry.RegisterHierarchyRebuild(notification.Taxon.TaxonomyId);
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonEvents.TaxonUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred hierarchy rebuild for Taxon UPDATED {TaxonId}", notification.Taxon.Id);
        registry.RegisterHierarchyRebuild(notification.Taxon.TaxonomyId);
        await Task.CompletedTask;
    }
}
