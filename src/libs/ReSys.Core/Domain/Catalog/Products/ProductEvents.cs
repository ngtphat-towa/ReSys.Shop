using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Domain.Catalog.Products;

public static class ProductEvents
{
    public record ProductCreated(Product Product) : IDomainEvent;
    public record ProductUpdated(Product Product) : IDomainEvent;
    public record VariantAdded(Product Product, Variants.Variant Variant) : IDomainEvent;
    public record ProductActivated(Product Product) : IDomainEvent;
    public record ProductArchived(Product Product) : IDomainEvent;
    
    // ML Triggering Event
    public record ImageAdded(Product Product, ProductImage Image) : IDomainEvent;
}