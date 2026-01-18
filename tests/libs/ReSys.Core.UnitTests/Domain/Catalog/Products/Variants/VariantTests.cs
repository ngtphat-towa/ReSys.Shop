using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products.Variants;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Variant")]
public class VariantTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var productId = Guid.NewGuid();
        var result = Variant.Create(productId, "SKU-1", 100);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ProductId.Should().Be(productId);
        result.Value.Sku.Should().Be("SKU-1");
        result.Value.Price.Should().Be(100);
        result.Value.DomainEvents.Should().ContainSingle(e => e is VariantEvents.VariantCreated);
    }

    [Fact(DisplayName = "UpdatePricing should validate compare-at price")]
    public void UpdatePricing_Should_ValidateComparePrice()
    {
        // Arrange
        var variant = Variant.Create(Guid.NewGuid(), "SKU", 100).Value;

        // Act
        var result = variant.UpdatePricing(100, 90); // CompareAt must be > Price

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VariantErrors.InvalidCompareAtPrice);
    }

    [Fact(DisplayName = "UpdateDimensions should modify physical attributes")]
    public void UpdateDimensions_Should_ChangePhysicalData()
    {
        // Arrange
        var variant = Variant.Create(Guid.NewGuid(), "SKU", 100).Value;

        // Act
        var result = variant.UpdateDimensions(1.5m, 10, 20, 30);

        // Assert
        result.IsError.Should().BeFalse();
        variant.Weight.Should().Be(1.5m);
        variant.Height.Should().Be(10);
        variant.Width.Should().Be(20);
        variant.Depth.Should().Be(30);
    }

    [Fact(DisplayName = "Delete should raise deleted event")]
    public void Delete_Should_SetDeletedState()
    {
        // Arrange
        var variant = Variant.Create(Guid.NewGuid(), "SKU", 100).Value;
        variant.ClearDomainEvents();

        // Act
        var result = variant.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        variant.IsDeleted.Should().BeTrue();
        variant.DomainEvents.Should().ContainSingle(e => e is VariantEvents.VariantDeleted);
    }
}