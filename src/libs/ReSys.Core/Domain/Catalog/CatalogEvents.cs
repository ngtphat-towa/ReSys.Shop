using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog;

public static class CatalogEvents
{
    // Product Lifecycle
    public record ProductCreated(Product Product) : IDomainEvent;
    public record ProductUpdated(Guid ProductId) : IDomainEvent;
    public record ProductViewed(Guid ProductId) : IDomainEvent;
    public record ProductStatusChanged(Guid ProductId, Product.ProductStatus OldStatus, Product.ProductStatus NewStatus) : IDomainEvent;
    public record ProductDeleted(Guid ProductId) : IDomainEvent;
    public record ProductRestored(Guid ProductId) : IDomainEvent;

    // Product Components
    public record ProductImageAdded(Guid ProductId, Guid ImageId) : IDomainEvent;
    public record ProductImageRemoved(Guid ProductId, Guid ImageId) : IDomainEvent;
    public record ProductOptionTypeAdded(Guid ProductId, Guid OptionTypeId) : IDomainEvent;
    public record ProductOptionTypeRemoved(Guid ProductId, Guid OptionTypeId) : IDomainEvent;
    public record ProductClassificationAdded(Guid ProductId, Guid TaxonId) : IDomainEvent;
    public record ProductClassificationRemoved(Guid ProductId, Guid TaxonId) : IDomainEvent;
    public record ProductPropertyAdded(Guid ProductId, Guid PropertyTypeId) : IDomainEvent;
    public record ProductPropertyRemoved(Guid ProductId, Guid PropertyTypeId) : IDomainEvent;

    // Variant Logic
    public record VariantAdded(Guid ProductId, Variant Variant) : IDomainEvent;
    public record VariantRemoved(Guid ProductId, Guid VariantId) : IDomainEvent;
    public record VariantRestored(Guid ProductId, Guid VariantId) : IDomainEvent;
    public record VariantStockChanged(Guid VariantId, double NewQuantity) : IDomainEvent;
    public record VariantUpdated(Guid VariantId) : IDomainEvent;

    // Option Types
    public record OptionTypeCreated(Guid OptionTypeId) : IDomainEvent;
    public record OptionTypeUpdated(Guid OptionTypeId) : IDomainEvent;

    // Taxonomy Hierarchy Logic
    public record TaxonCreated(Guid TaxonId) : IDomainEvent;
    public record TaxonUpdated(Guid TaxonId) : IDomainEvent;
    public record TaxonMoved(Guid TaxonId, Guid? NewParentId) : IDomainEvent;
    public record TaxonProductsRegenerationRequested(Guid TaxonId) : IDomainEvent;
    public record TaxonDeleted(Guid TaxonId) : IDomainEvent;
}
