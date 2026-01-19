using Microsoft.Extensions.Logging.Abstractions;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonRegenerationServiceDetailedTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonRegenerationService _service = new(fixture.Context, NullLogger<TaxonRegenerationService>.Instance);

    [Fact(DisplayName = "Regenerate: Should match by variant SKU starts_with")]
    public async Task Regenerate_ShouldMatch_ByVariantSku()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("SkuTest").Value;
        var taxon = taxonomy.AddTaxon("Apples").Value;
        taxon.Automatic = true;
        
        var rule = taxon.AddRule("variant_sku", "APL-", "starts_with").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy); // Track the taxonomy (and taxon + rules)

        var p1 = Product.Create("Apple Red", "APL-RED", 1).Value;
        p1.Activate();
        var p2 = Product.Create("Banana", "BAN-001", 1).Value;
        p2.Activate();

        fixture.Context.Set<Product>().AddRange(p1, p2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications.First().ProductId.Should().Be(p1.Id);
    }

    [Fact(DisplayName = "Regenerate: Should match by variant Price greater_than")]
    public async Task Regenerate_ShouldMatch_ByVariantPrice()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("PriceTest").Value;
        var taxon = taxonomy.AddTaxon("Premium").Value;
        taxon.Automatic = true;
        taxon.AddRule("variant_price", "100", "greater_than");

        var p1 = Product.Create("Expensive", "EXP", 150).Value;
        p1.Activate();
        var p2 = Product.Create("Cheap", "CHP", 50).Value;
        p2.Activate();

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<Product>().AddRange(p1, p2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications.First().ProductId.Should().Be(p1.Id);
    }

    [Fact(DisplayName = "Regenerate: Should match by Product Property")]
    public async Task Regenerate_ShouldMatch_ByProductProperty()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("PropTest").Value;
        var taxon = taxonomy.AddTaxon("Cotton").Value;
        taxon.Automatic = true;
        taxon.AddRule("product_property", "100% Cotton", "is_equal_to", "Material");

        var propType = PropertyType.Create("Material", "String").Value;
        
        var p1 = Product.Create("T-Shirt", "TSH", 20).Value;
        p1.SetPropertyValue(propType, "100% Cotton");
        p1.Activate();

        var p2 = Product.Create("Jeans", "JNS", 50).Value;
        p2.SetPropertyValue(propType, "Denim");
        p2.Activate();

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<PropertyType>().Add(propType);
        fixture.Context.Set<Product>().AddRange(p1, p2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications.First().ProductId.Should().Be(p1.Id);
    }

    [Fact(DisplayName = "Regenerate: Should respect 'any' match policy")]
    public async Task Regenerate_ShouldRespect_AnyPolicy()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("AnyPolicyTest").Value;
        var taxon = taxonomy.AddTaxon("Mixed").Value;
        taxon.Automatic = true;
        taxon.RulesMatchPolicy = "any";
        taxon.AddRule("product_name", "UniqueRed", "contains");
        taxon.AddRule("product_name", "UniqueBlue", "contains");

        var p1 = Product.Create("UniqueRed Shirt", "URED", 10).Value;
        p1.Activate();
        var p2 = Product.Create("UniqueBlue Shirt", "UBLU", 10).Value;
        p2.Activate();
        var p3 = Product.Create("UniqueGreen Shirt", "UGRN", 10).Value;
        p3.Activate();

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<Product>().AddRange(p1, p2, p3);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .Select(c => c.ProductId)
            .ToList();

        classifications.Should().HaveCount(2);
        classifications.Should().Contain(p1.Id);
        classifications.Should().Contain(p2.Id);
        classifications.Should().NotContain(p3.Id);
    }

    [Fact(DisplayName = "Regenerate: Should respect 'all' match policy")]
    public async Task Regenerate_ShouldRespect_AllPolicy()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("AllPolicyTest").Value;
        var taxon = taxonomy.AddTaxon("RedCotton").Value;
        taxon.Automatic = true;
        taxon.RulesMatchPolicy = "all";
        
        var propType = PropertyType.Create("Material", "String").Value;

        taxon.AddRule("product_name", "Red", "contains");
        taxon.AddRule("product_property", "Cotton", "is_equal_to", "Material");

        var p1 = Product.Create("Red Cotton Shirt", "RCS", 10).Value;
        p1.SetPropertyValue(propType, "Cotton");
        p1.Activate();

        var p2 = Product.Create("Red Silk Shirt", "RSS", 20).Value;
        p2.SetPropertyValue(propType, "Silk");
        p2.Activate();

        var p3 = Product.Create("Blue Cotton Shirt", "BCS", 10).Value;
        p3.SetPropertyValue(propType, "Cotton");
        p3.Activate();

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<PropertyType>().Add(propType);
        fixture.Context.Set<Product>().AddRange(p1, p2, p3);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .Select(c => c.ProductId)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications.Should().Contain(p1.Id);
    }
}
