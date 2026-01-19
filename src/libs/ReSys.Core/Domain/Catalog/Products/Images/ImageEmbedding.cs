using ReSys.Core.Domain.Common.Abstractions;

using Pgvector;

namespace ReSys.Core.Domain.Catalog.Products.Images;

public sealed class ImageEmbedding : Entity
{
    public Guid ProductImageId { get; set; }
    public ProductImage ProductImage { get; set; } = null!; // Navigation

    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// Native PostgreSQL Vector type for high-performance semantic search.
    /// </summary>
    public Vector Vector { get; set; } = null!;
    public int Dimensions { get; set; }

    private ImageEmbedding() { }

    public static ImageEmbedding Create(Guid productImageId, string modelName, string version, float[] vectorData)
    {
        return new ImageEmbedding
        {
            Id = Guid.NewGuid(),
            ProductImageId = productImageId,
            ModelName = modelName.Trim(),
            ModelVersion = version.Trim(),
            Vector = new Vector(vectorData),
            Dimensions = vectorData.Length
        };
    }
}
