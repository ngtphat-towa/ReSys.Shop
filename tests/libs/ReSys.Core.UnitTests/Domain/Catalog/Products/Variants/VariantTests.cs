using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Variant")]
public class VariantTests
{
    private readonly Guid _productId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize variant")]
    public void Create_Should_InitializeVariant()
    {
        // Act
        var result = Variant.Create(_productId, "SKU-001", 150.00m, isMaster: false);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ProductId.Should().Be(_productId);
        result.Value.Sku.Should().Be("SKU-001");
        result.Value.Price.Should().Be(150.00m);
        result.Value.IsMaster.Should().BeFalse();
        result.Value.DomainEvents.Should().ContainSingle(e => e is VariantEvents.VariantCreated);
    }

    [Fact(DisplayName = "UpdatePricing: Should update prices and validate compare-at logic")]
    public void UpdatePricing_Should_UpdateAndValidate()
    {
        // Arrange
        var variant = Variant.Create(_productId, "SKU", 100).Value;

        // Act: Valid update
        var result = variant.UpdatePricing(90, 110);

        // Assert
        result.IsError.Should().BeFalse();
        variant.Price.Should().Be(90);
        variant.CompareAtPrice.Should().Be(110);

        // Act: Invalid (CompareAt <= Price)
        var invalidResult = variant.UpdatePricing(100, 90);

        // Assert
        invalidResult.IsError.Should().BeTrue();
        invalidResult.FirstError.Should().Be(VariantErrors.InvalidCompareAtPrice);
    }

    [Fact(DisplayName = "UpdateDimensions: Should change physical attributes")]
    public void UpdateDimensions_Should_ChangeAttributes()
    {
        // Arrange
        var variant = Variant.Create(_productId, "SKU", 100).Value;

        // Act
        var result = variant.UpdateDimensions(2.5m, 15.5m, 20.0m, 5.0m);

        // Assert
        result.IsError.Should().BeFalse();
        variant.Weight.Should().Be(2.5m);
        variant.Height.Should().Be(15.5m);
        variant.Width.Should().Be(20.0m);
        variant.Depth.Should().Be(5.0m);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetSoftDelete()
    {
        // Arrange
        var variant = Variant.Create(_productId, "SKU", 100).Value;

        // Act
        var result = variant.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        variant.IsDeleted.Should().BeTrue();
        variant.DeletedAt.Should().NotBeNull();
        variant.DomainEvents.Should().Contain(e => e is VariantEvents.VariantDeleted);
    }
}