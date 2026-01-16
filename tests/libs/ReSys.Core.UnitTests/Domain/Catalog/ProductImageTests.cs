using FluentAssertions;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class ProductImageTests
{
    [Fact(DisplayName = "AddOrUpdateEmbedding should manage multiple model vectors")]
    public void AddOrUpdateEmbedding_ShouldManage_MultipleModels()
    {
        // Arrange
        var image = ProductImage.Create(Guid.NewGuid(), "url").Value;
        var clipVector = new float[] { 0.1f, 0.2f };
        var dinoVector = new float[] { 0.9f, 0.8f, 0.7f };

        // Act
        image.AddOrUpdateEmbedding("fashion-clip", "v1", clipVector);
        image.AddOrUpdateEmbedding("dinov2", "v1", dinoVector);

        // Assert
        image.Embeddings.Should().HaveCount(2);
        image.Status.Should().Be(ProductImage.ImageProcessingStatus.Processed);
        
        var clip = image.Embeddings.First(e => e.ModelName == "fashion-clip");
        clip.Vector.Should().BeEquivalentTo(clipVector);
        clip.Dimensions.Should().Be(2);
    }

    [Fact(DisplayName = "AddOrUpdateEmbedding should replace existing model embedding")]
    public void AddOrUpdateEmbedding_ShouldReplace_Existing()
    {
        // Arrange
        var image = ProductImage.Create(Guid.NewGuid(), "url").Value;
        image.AddOrUpdateEmbedding("cnn", "v1", [1f]);

        // Act
        image.AddOrUpdateEmbedding("cnn", "v2", [2f, 3f]);

        // Assert
        image.Embeddings.Should().HaveCount(1);
        image.Embeddings.First().ModelVersion.Should().Be("v2");
        image.Embeddings.First().Dimensions.Should().Be(2);
    }
}
