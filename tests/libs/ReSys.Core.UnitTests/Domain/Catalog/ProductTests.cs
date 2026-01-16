using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class ProductTests
{
    [Fact(DisplayName = "Create should succeed and initialize master variant")]
    public void Create_ShouldSucceed_AndInitializeMasterVariant()
    {
        // Arrange
        var name = "Denim Jacket";
        var sku = "DJ-001";
        var price = 89.99m;

        // Act
        var result = Product.Create(name, sku, price);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(name);
        result.Value.Variants.Should().HaveCount(1);
        result.Value.Variants.First().IsMaster.Should().BeTrue();
        result.Value.Variants.First().Sku.Should().Be(sku);
        result.Value.DomainEvents.Should().ContainSingle(e => e is ProductEvents.ProductCreated);
    }

    [Fact(DisplayName = "AddVariant should add new variant and raise event")]
    public void AddVariant_ShouldAddVariant_AndRaiseEvent()
    {
        // Arrange
        var product = Product.Create("T-Shirt", "TS-001", 19.99m).Value;
        product.ClearDomainEvents();

        // Act
        var result = product.AddVariant("TS-002", 24.99m);

        // Assert
        result.IsError.Should().BeFalse();
        product.Variants.Should().HaveCount(2);
        product.DomainEvents.Should().ContainSingle(e => e is ProductEvents.VariantAdded);
    }

    [Fact(DisplayName = "AddImage should manage roles and enforce invariants")]
    public void AddImage_ShouldManageRoles_AndEnforceInvariants()
    {
        // Arrange
        var product = Product.Create("Shoe", "SH-001", 50m).Value;

        // Act 1: Add first image (should be Default automatically)
        var img1 = product.AddImage("url1", "alt1").Value;

        // Act 2: Add second image as Search
        var img2 = product.AddImage("url2", "alt2", role: ProductImage.ProductImageType.Search).Value;

        // Act 3: Add third image as Default (should demote img1)
        var img3 = product.AddImage("url3", "alt3", role: ProductImage.ProductImageType.Default).Value;

        // Assert
        img1.Role.Should().Be(ProductImage.ProductImageType.Gallery); // Demoted
        img2.Role.Should().Be(ProductImage.ProductImageType.Search);
        img3.Role.Should().Be(ProductImage.ProductImageType.Default);
        product.Images.Should().HaveCount(3);
        product.DomainEvents.Should().Contain(e => e is ProductEvents.ImageAdded);
    }

    [Fact(DisplayName = "Activate should change status and set availability date")]
    public void Activate_ShouldChangeStatus_AndSetDate()
    {
        // Arrange
        var product = Product.Create("Hat", "H-001", 10m).Value;

        // Act
        product.Activate();

        // Assert
        product.Status.Should().Be(Product.ProductStatus.Active);
        product.AvailableOn.Should().NotBeNull();
        product.DomainEvents.Should().Contain(e => e is ProductEvents.ProductActivated);
    }
}
