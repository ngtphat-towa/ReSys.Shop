using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Products.Variants;

public static class VariantEvents
{
    public record VariantCreated(Variant Variant) : IDomainEvent;
    public record VariantUpdated(Variant Variant) : IDomainEvent;
    public record VariantDeleted(Variant Variant) : IDomainEvent;
}
