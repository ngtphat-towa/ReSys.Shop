using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

public static class TaxonRuleEvents
{
    public record TaxonRuleAdded(Taxon Taxon, TaxonRule Rule) : IDomainEvent;
    public record TaxonRuleUpdated(Taxon Taxon, TaxonRule Rule) : IDomainEvent;
    public record TaxonRuleRemoved(Taxon Taxon, TaxonRule Rule) : IDomainEvent;
}
