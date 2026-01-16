using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class VariantTests
{
    [Fact(DisplayName = "Create should validate non-negative price")]
    public void Create_ShouldValidate_NonNegativePrice()
    {
        var result = Variant.Create(Guid.NewGuid(), "SKU-001", -10m);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VariantErrors.InvalidPrice);
    }

    [Fact(DisplayName = "UpdatePricing should validate compare-at price")]
    public void UpdatePricing_ShouldValidate_CompareAtPrice()
    {
        // Arrange
        var variant = Variant.Create(Guid.NewGuid(), "SKU-001", 100m).Value;

        // Act: Set compare-at price lower than current price
        var result = variant.UpdatePricing(100m, 50m);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Variant.InvalidCompareAtPrice");
    }

    [Fact(DisplayName = "UpdateDimensions should modify physical attributes")]
    public void UpdateDimensions_ShouldModify_Attributes()
    {
        // Arrange
        var variant = Variant.Create(Guid.NewGuid(), "SKU-001", 10m).Value;

        // Act
        variant.UpdateDimensions(1.5m, 10m, 20m, 5m);

        // Assert
        variant.Weight.Should().Be(1.5m);
        variant.Height.Should().Be(10m);
        variant.Width.Should().Be(20m);
        variant.Depth.Should().Be(5m);
    }
}
