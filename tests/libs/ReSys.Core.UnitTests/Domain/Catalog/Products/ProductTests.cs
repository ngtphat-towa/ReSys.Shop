using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Product")]
public class ProductTests
{
    [Fact(DisplayName = "Create should succeed and initialize master variant")]
    public void Create_ShouldSucceed_AndInitializeMasterVariant()
    {
        // Act
        var result = Product.Create("Laptop", "LAP", 1000, "laptop", "Description");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Laptop");
        result.Value.MasterVariant.Should().NotBeNull();
        result.Value.MasterVariant!.IsMaster.Should().BeTrue();
        result.Value.MasterVariant.Price.Should().Be(1000);
        result.Value.MasterVariant.Sku.Should().Be("LAP");
        result.Value.DomainEvents.Should().ContainSingle(e => e is ProductEvents.ProductCreated);
    }

    [Fact(DisplayName = "Create should fail when name is empty")]
    public void Create_ShouldFail_WhenNameIsEmpty()
    {
        var result = Product.Create("", "SKU", 100);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProductErrors.NameRequired);
    }

    [Fact(DisplayName = "Create should fail when price is invalid")]
    public void Create_ShouldFail_WhenPriceIsInvalid()
    {
        var result = Product.Create("Product", "SKU", -1);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProductErrors.InvalidPrice);
    }

    [Fact(DisplayName = "Activate should change status and set availability date")]
    public void Activate_Should_ChangeStatus()
    {
        // Arrange
        var product = Product.Create("Product", "SKU", 100).Value;
        product.Status.Should().Be(Product.ProductStatus.Draft);

        // Act
        var result = product.Activate();

        // Assert
        result.IsError.Should().BeFalse();
        product.Status.Should().Be(Product.ProductStatus.Active);
        product.AvailableOn.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddVariant should add new variant and raise event")]
    public void AddVariant_Should_AddNewVariant()
    {
        // Arrange
        var product = Product.Create("Product", "MASTER", 100).Value;
        product.ClearDomainEvents();

        // Act
        var result = product.AddVariant("VAR-1", 120);

        // Assert
        result.IsError.Should().BeFalse();
        product.Variants.Should().HaveCount(2);
        result.Value.Sku.Should().Be("VAR-1");
        product.DomainEvents.Should().ContainSingle(e => e is ProductEvents.VariantAdded);
    }

    [Fact(DisplayName = "AddVariant should fail if SKU is duplicate")]
    public void AddVariant_ShouldFail_IfSkuDuplicate()
    {
        // Arrange
        var product = Product.Create("Product", "MASTER", 100).Value;

        // Act
        var result = product.AddVariant("MASTER", 120);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProductErrors.DuplicateSku);
    }

    [Fact(DisplayName = "SetMasterVariant should update master status across variants")]
    public void SetMasterVariant_Should_SwitchMaster()
    {
        // Arrange
        var product = Product.Create("Product", "V1", 100).Value;
        var v2 = product.AddVariant("V2", 110).Value;
        
        product.MasterVariant!.Sku.Should().Be("V1");

        // Act
        var result = product.SetMasterVariant(v2.Id);

        // Assert
        result.IsError.Should().BeFalse();
        product.MasterVariant.Id.Should().Be(v2.Id);
        product.Variants.First(x => x.Sku == "V1").IsMaster.Should().BeFalse();
    }
}
