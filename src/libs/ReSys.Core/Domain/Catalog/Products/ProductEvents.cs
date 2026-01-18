using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Domain.Catalog.Products;

public static class ProductEvents
{
    public record ProductCreated(Product Product) : IDomainEvent;
    public record ProductUpdated(Product Product) : IDomainEvent;
    public record ProductDeleted(Product Product) : IDomainEvent;
    public record ProductRestored(Product Product) : IDomainEvent;
    
    // Status
    public record ProductStatusChanged(Product Product, Product.ProductStatus OldStatus, Product.ProductStatus NewStatus) : IDomainEvent;
    
    // Variants
    public record VariantAdded(Product Product, Variant Variant) : IDomainEvent;
    public record VariantRemoved(Product Product, Variant Variant) : IDomainEvent;
    public record VariantRestored(Product Product, Variant Variant) : IDomainEvent;
    
    // Images
    public record ImageAdded(Product Product, ProductImage Image) : IDomainEvent;
    public record ImageRemoved(Product Product, ProductImage Image) : IDomainEvent;
    
    // Classifications
    public record ClassificationAdded(Product Product, Classification Classification) : IDomainEvent;
    public record ClassificationRemoved(Product Product, Classification Classification) : IDomainEvent;
    
    // Taxonomies
    public record ProductTaxonRulesRegenerationRequired(Product Product) : IDomainEvent;
}
