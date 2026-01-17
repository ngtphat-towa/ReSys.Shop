using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Products.Images;

public sealed class ProductImage : Aggregate, IHasMetadata
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public int Position { get; set; }
    public ProductImageType Role { get; set; } = ProductImageType.Gallery;

    // ML Status
    public ImageProcessingStatus Status { get; set; } = ImageProcessingStatus.Pending;

    // Multi-Model Embeddings
    public ICollection<ImageEmbedding> Embeddings { get; set; } = new List<ImageEmbedding>();

    // Media metadata
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private ProductImage() { }

    public static ErrorOr<ProductImage> Create(
        Guid productId,
        string url,
        string? alt = null,
        Guid? variantId = null,
        ProductImageType role = ProductImageType.Gallery)
    {
        if (string.IsNullOrWhiteSpace(url)) return ProductImageErrors.UrlRequired;
        if (url.Length > ProductImageConstraints.UrlMaxLength) return ProductImageErrors.UrlTooLong;

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VariantId = variantId,
            Url = url.Trim(),
            Alt = alt?.Trim(),
            Role = role,
            Status = ImageProcessingStatus.Pending
        };

        image.RaiseDomainEvent(new ProductImageEvents.ImageUploaded(image));
        return image;
    }

    public void MarkAsProcessing()
    {
        if (Status == ImageProcessingStatus.Processed) return;
        Status = ImageProcessingStatus.Processing;
        RaiseDomainEvent(new ProductImageEvents.ImageProcessingStarted(this));
    }

    public void AddOrUpdateEmbedding(string modelName, string version, float[] vectorData)
    {
        var existing = Embeddings.FirstOrDefault(e => e.ModelName == modelName);
        if (existing != null)
        {
            Embeddings.Remove(existing);
        }

        var embedding = ImageEmbedding.Create(Id, modelName, version, vectorData);
        Embeddings.Add(embedding);

        Status = ImageProcessingStatus.Processed;
        RaiseDomainEvent(new ProductImageEvents.EmbeddingGenerated(this, embedding));
    }

    public void MarkAsFailed(string reason)
    {
        Status = ImageProcessingStatus.Failed;
        RaiseDomainEvent(new ProductImageEvents.ImageProcessingFailed(this, reason));
    }

    public void UpdateMetadata(string? contentType, long? fileSize, int? width, int? height)
    {
        ContentType = contentType;
        FileSize = fileSize;
        Width = width;
        Height = height;
    }

    public void Update(string? alt, int position)
    {
        if (alt?.Length > ProductImageConstraints.AltMaxLength) return;
        Alt = alt?.Trim();
        Position = position;
    }

    public void SetRole(ProductImageType role)
    {
        Role = role;
    }

    public enum ProductImageType
    {
        Default,    // Primary display image
        Thumbnail,  // Small preview
        Square,     // Fixed 1:1 aspect ratio
        Gallery,    // High-resolution detail view
        Search      // AI semantic search source
    }

    public enum ImageProcessingStatus { Pending, Processing, Processed, Failed }
}
