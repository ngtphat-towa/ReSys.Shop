using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Products.Images;

public static class ProductImageEvents
{
    public record ImageUploaded(ProductImage Image) : IDomainEvent;
    public record ImageProcessingStarted(ProductImage Image) : IDomainEvent;
    public record EmbeddingGenerated(ProductImage Image, ImageEmbedding Embedding) : IDomainEvent;
    public record ImageProcessingFailed(ProductImage Image, string Reason) : IDomainEvent;
}