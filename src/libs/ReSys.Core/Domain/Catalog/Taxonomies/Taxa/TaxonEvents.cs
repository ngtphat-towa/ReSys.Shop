using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public static class TaxonEvents
{
    public record TaxonCreated(Taxon Taxon) : IDomainEvent;
    public record TaxonUpdated(Taxon Taxon) : IDomainEvent;
    public record TaxonDeleted(Taxon Taxon) : IDomainEvent;
    public record TaxonMoved(Taxon Taxon, Guid? OldParentId, Guid? NewParentId) : IDomainEvent;
}
