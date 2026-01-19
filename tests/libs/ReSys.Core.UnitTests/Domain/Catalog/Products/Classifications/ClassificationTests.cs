using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products.Classifications;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Classification")]
public class ClassificationTests
{
    [Fact(DisplayName = "Create: Should successfully initialize classification")]
    public void Create_Should_InitializeClassification()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var taxonId = Guid.NewGuid();

        // Act
        var result = Classification.Create(productId, taxonId, 10, isAutomatic: true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ProductId.Should().Be(productId);
        result.Value.TaxonId.Should().Be(taxonId);
        result.Value.Position.Should().Be(10);
        result.Value.IsAutomatic.Should().BeTrue();
        result.Value.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "UpdatePosition: Should change sort order and update timestamp")]
    public void UpdatePosition_Should_ChangeSortOrder()
    {
        // Arrange
        var classification = Classification.Create(Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act
        classification.UpdatePosition(25);

        // Assert
        classification.Position.Should().Be(25);
        classification.UpdatedAt.Should().NotBeNull();
    }
}
