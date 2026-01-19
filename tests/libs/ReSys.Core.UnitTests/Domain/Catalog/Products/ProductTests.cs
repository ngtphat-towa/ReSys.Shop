using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Core.UnitTests.Domain.Catalog.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Product")]
public class ProductTests
{
    private const string ValidSku = "PROD-123";
    private const decimal ValidPrice = 99.99m;

    [Fact(DisplayName = "Create: Should successfully initialize product and master variant")]
    public void Create_Should_InitializeProductAndMasterVariant()
    {
        // Act
        var result = Product.Create("Smartphone", ValidSku, ValidPrice, description: "Latest model");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Smartphone");
        result.Value.MasterVariant.Should().NotBeNull();
        result.Value.MasterVariant!.Sku.Should().Be(ValidSku);
        result.Value.MasterVariant.Price.Should().Be(ValidPrice);
        result.Value.MasterVariant.IsMaster.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is ProductEvents.ProductCreated);
    }

    [Fact(DisplayName = "UpdateSeo: Should update SEO metadata with length validation")]
    public void UpdateSeo_Should_UpdateMetadata()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;

        // Act
        var result = product.UpdateSeo("Title", "Description", "Keywords");

        // Assert
        result.IsError.Should().BeFalse();
        product.MetaTitle.Should().Be("Title");
        product.MetaDescription.Should().Be("Description");
        product.MetaKeywords.Should().Be("Keywords");
    }

    [Fact(DisplayName = "UpdateSeo: Should fail if title is too long")]
    public void UpdateSeo_ShouldFail_IfTitleTooLong()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;
        var longTitle = new string('A', ProductConstraints.Seo.MetaTitleMaxLength + 1);

        // Act
        var result = product.UpdateSeo(longTitle, null, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProductErrors.Seo.MetaTitleTooLong);
    }

    [Fact(DisplayName = "Activate: Should change status and set availability date")]
    public void Activate_Should_SetStatusToActive()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;

        // Act
        var result = product.Activate();

        // Assert
        result.IsError.Should().BeFalse();
        product.Status.Should().Be(Product.ProductStatus.Active);
        product.AvailableOn.Should().NotBeNull();
        product.DomainEvents.Should().Contain(e => e is ProductEvents.ProductStatusChanged);
    }

    [Fact(DisplayName = "AddVariant: Should successfully add non-master variant")]
    public void AddVariant_Should_AddVariant()
    {
        // Arrange
        var product = Product.Create("Product", "MASTER", ValidPrice).Value;
        product.ClearDomainEvents();

        // Act
        var result = product.AddVariant("VAR-1", 120.00m);

        // Assert
        result.IsError.Should().BeFalse();
        product.Variants.Should().HaveCount(2);
        result.Value.IsMaster.Should().BeFalse();
        product.DomainEvents.Should().ContainSingle(e => e is ProductEvents.VariantAdded);
    }

    [Fact(DisplayName = "AddVariant: Should fail if SKU already exists")]
    public void AddVariant_ShouldFail_IfDuplicateSku()
    {
        // Arrange
        var product = Product.Create("Product", "MASTER", ValidPrice).Value;

        // Act
        var result = product.AddVariant("MASTER", 150.00m);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProductErrors.DuplicateSku);
    }

    [Fact(DisplayName = "SetMasterVariant: Should switch master status between variants")]
    public void SetMasterVariant_Should_SwitchMaster()
    {
        // Arrange
        var product = Product.Create("Product", "V1", 100).Value;
        var v2 = product.AddVariant("V2", 110).Value;

        // Act
        var result = product.SetMasterVariant(v2.Id);

        // Assert
        result.IsError.Should().BeFalse();
        product.MasterVariant!.Id.Should().Be(v2.Id);
        product.Variants.First(v => v.Sku == "V1").IsMaster.Should().BeFalse();
    }

    [Fact(DisplayName = "AddClassification: Should associate taxon and mark for regeneration")]
    public void AddClassification_Should_AssociateTaxon()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;
        var taxonId = Guid.NewGuid();

        // Act
        var result = product.AddClassification(taxonId, 1);

        // Assert
        result.IsError.Should().BeFalse();
        product.Classifications.Should().ContainSingle(c => c.TaxonId == taxonId);
        product.MarkedForRegenerateTaxonProducts.Should().BeTrue();
    }

    [Fact(DisplayName = "AddImage: Should set first gallery image as default")]
    public void AddImage_Should_SetFirstAsDefault()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;

        // Act
        var image = product.AddImage("https://example.com/img.jpg", "Alt Text").Value;

        // Assert
        image.Role.Should().Be(ProductImage.ProductImageType.Default);
        product.Images.Should().Contain(image);
    }

    [Fact(DisplayName = "SetPropertyValue: Should add or update product property")]
    public void SetPropertyValue_Should_ManageProperties()
    {
        // Arrange
        var product = Product.Create("Product", ValidSku, ValidPrice).Value;
        var propType = PropertyType.Create("Material", "Material", PropertyKind.String).Value;

        // Act: Add new
        product.SetPropertyValue(propType, "Cotton");

        // Assert
        product.ProductProperties.Should().ContainSingle(p => p.Value == "Cotton");

        // Act: Update existing
        product.SetPropertyValue(propType, "Silk");

        // Assert
        product.ProductProperties.Should().ContainSingle(p => p.Value == "Silk");
    }
}
