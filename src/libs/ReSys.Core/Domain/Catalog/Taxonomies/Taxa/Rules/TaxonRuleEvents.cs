using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

public static class TaxonRuleEvents
{
    public record TaxonRuleCreated(TaxonRule Rule) : IDomainEvent;
    public record TaxonRuleUpdated(TaxonRule Rule) : IDomainEvent;
    public record TaxonRuleDeleted(TaxonRule Rule) : IDomainEvent;
}