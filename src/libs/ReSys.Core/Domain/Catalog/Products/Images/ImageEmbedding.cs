using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.Products.Images;

public sealed class ImageEmbedding : Entity
{
    public Guid ProductImageId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public float[] Vector { get; set; } = [];
    public int Dimensions { get; set; }

    private ImageEmbedding() { }

    public static ImageEmbedding Create(string modelName, string version, float[] vector)
    {
        return new ImageEmbedding
        {
            ModelName = modelName,
            ModelVersion = version,
            Vector = vector,
            Dimensions = vector.Length
        };
    }
}