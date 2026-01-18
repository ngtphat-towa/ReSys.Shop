using MediatR;
using Microsoft.Extensions.Logging;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.Features.Catalog.Taxonomies.Services;

public class TaxonRegenerationEventHandler(
    ITaxonWorkRegistry registry,
    ILogger<TaxonRegenerationEventHandler> logger) :
    INotificationHandler<TaxonEvents.TaxonCreated>,
    INotificationHandler<TaxonEvents.TaxonUpdated>,
    INotificationHandler<TaxonRuleEvents.TaxonRuleAdded>,
    INotificationHandler<TaxonRuleEvents.TaxonRuleUpdated>,
    INotificationHandler<TaxonRuleEvents.TaxonRuleRemoved>
{
    public async Task Handle(TaxonEvents.TaxonCreated notification, CancellationToken cancellationToken)
    {
        if (notification.Taxon.Automatic)
        {
            logger.LogDebug("[EVENT] Registering deferred regeneration for NEW automatic taxon {TaxonId}", notification.Taxon.Id);
            registry.RegisterProductRegeneration(notification.Taxon.Id);
        }
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonEvents.TaxonUpdated notification, CancellationToken cancellationToken)
    {
        if (notification.Taxon.Automatic)
        {
            logger.LogDebug("[EVENT] Registering deferred regeneration for UPDATED automatic taxon {TaxonId}", notification.Taxon.Id);
            registry.RegisterProductRegeneration(notification.Taxon.Id);
        }
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonRuleEvents.TaxonRuleAdded notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred regeneration for Taxon {TaxonId} due to Rule ADDED", notification.Taxon.Id);
        registry.RegisterProductRegeneration(notification.Taxon.Id);
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonRuleEvents.TaxonRuleUpdated notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred regeneration for Taxon {TaxonId} due to Rule UPDATED", notification.Taxon.Id);
        registry.RegisterProductRegeneration(notification.Taxon.Id);
        await Task.CompletedTask;
    }

    public async Task Handle(TaxonRuleEvents.TaxonRuleRemoved notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("[EVENT] Registering deferred regeneration for Taxon {TaxonId} due to Rule REMOVED", notification.Taxon.Id);
        registry.RegisterProductRegeneration(notification.Taxon.Id);
        await Task.CompletedTask;
    }
}
