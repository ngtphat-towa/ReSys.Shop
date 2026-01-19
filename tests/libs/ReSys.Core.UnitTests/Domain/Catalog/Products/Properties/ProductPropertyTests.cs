using ReSys.Core.Domain.Catalog.Products.Properties;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products.Properties;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "ProductProperty")]
public class ProductPropertyTests
{
    [Fact(DisplayName = "Create: Should successfully initialize property and trim value")]
    public void Create_Should_InitializeProperty()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var propTypeId = Guid.NewGuid();

        // Act
        var result = ProductProperty.Create(productId, propTypeId, " Cotton ");

        // Assert
        result.ProductId.Should().Be(productId);
        result.PropertyTypeId.Should().Be(propTypeId);
        result.Value.Should().Be("Cotton");
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "UpdateValue: Should change value and update timestamp")]
    public void UpdateValue_Should_ChangeValue()
    {
        // Arrange
        var property = ProductProperty.Create(Guid.NewGuid(), Guid.NewGuid(), "Old");

        // Act
        property.UpdateValue("New");

        // Assert
        property.Value.Should().Be("New");
        property.UpdatedAt.Should().NotBeNull();
    }
}
