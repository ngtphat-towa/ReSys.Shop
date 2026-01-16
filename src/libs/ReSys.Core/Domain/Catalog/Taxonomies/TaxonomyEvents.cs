using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Taxonomies;

public static class TaxonomyEvents
{
    public record TaxonomyCreated(Taxonomy Taxonomy) : IDomainEvent;
    public record TaxonomyUpdated(Taxonomy Taxonomy) : IDomainEvent;
    public record TaxonomyDeleted(Taxonomy Taxonomy) : IDomainEvent;
}
