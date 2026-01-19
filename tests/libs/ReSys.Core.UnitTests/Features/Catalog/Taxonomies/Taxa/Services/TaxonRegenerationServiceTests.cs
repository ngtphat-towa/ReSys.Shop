using Microsoft.Extensions.Logging.Abstractions;

using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonRegenerationServiceTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonRegenerationService _service = new(fixture.Context, NullLogger<TaxonRegenerationService>.Instance);

    [Fact(DisplayName = "RegenerateProductsForTaxon should match products by name rule")]
    public async Task Regenerate_ShouldMatch_ByName()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("RegenTest").Value;
        var root = taxonomy.RootTaxon!;
        var automaticTaxon = taxonomy.AddTaxon("SmartPhones").Value;
        automaticTaxon.Automatic = true;
        automaticTaxon.RulesMatchPolicy = "all";
        automaticTaxon.AddRule("product_name", "iPhone", "contains");

        var p1 = Product.Create("iPhone 15", "IPH15", 999).Value;
        p1.Activate();
        var p2 = Product.Create("Samsung S24", "SAM24", 899).Value;
        p2.Activate();

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<Product>().AddRange(p1, p2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(automaticTaxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == automaticTaxon.Id)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications[0].ProductId.Should().Be(p1.Id);
    }

    [Fact(DisplayName = "RegenerateProductsForTaxon should sync removals when rules change")]
    public async Task Regenerate_ShouldSync_Removals()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("SyncTest").Value;
        var taxon = taxonomy.AddTaxon("CheapItems").Value;
        taxon.Automatic = true;
        taxon.AddRule("variant_price", "50", "less_than");

        var p1 = Product.Create("Pencil", "PNC", 10).Value; // Matches
        p1.Activate();
        var p2 = Product.Create("Laptop", "LAP", 1000).Value; // Doesn't match
        p2.Activate();

        // Manually add an incorrect automatic classification to test if it gets removed
        var oldClass = Classification.Create(p2.Id, taxon.Id, isAutomatic: true).Value;

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        fixture.Context.Set<Product>().AddRange(p1, p2);
        fixture.Context.Set<Classification>().Add(oldClass);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _service.RegenerateProductsForTaxonAsync(taxon.Id, TestContext.Current.CancellationToken);

        // Assert
        var classifications = fixture.Context.Set<Classification>()
            .Where(c => c.TaxonId == taxon.Id)
            .ToList();

        classifications.Should().HaveCount(1);
        classifications[0].ProductId.Should().Be(p1.Id);
        classifications.Any(c => c.ProductId == p2.Id).Should().BeFalse();
    }
}