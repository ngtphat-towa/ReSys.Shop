using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonomyTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var result = Taxonomy.Create("Categories", "Product Categories");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Categories");
        result.Value.DomainEvents.Should().ContainSingle(e => e is TaxonomyEvents.TaxonomyCreated);
    }

    [Fact(DisplayName = "AddTaxon should create new taxon associated with taxonomy")]
    public void AddTaxon_ShouldCreate_AssociatedTaxon()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Departments").Value;

        // Act
        var result = taxonomy.AddTaxon("Electronics");

        // Assert
        result.IsError.Should().BeFalse();
        taxonomy.Taxons.Should().HaveCount(1);
        result.Value.TaxonomyId.Should().Be(taxonomy.Id);
        result.Value.Name.Should().Be("Electronics");
        result.Value.Permalink.Should().BeEmpty(); // Not calculated yet
    }

    [Fact(DisplayName = "Taxon UpdatePermalink should build path correctly")]
    public void Taxon_UpdatePermalink_ShouldBuildPath()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Clothing").Value;
        var rootTaxon = taxonomy.AddTaxon("Men").Value;
        var subTaxon = Taxon.Create(taxonomy.Id, "Shoes", parentId: rootTaxon.Id).Value;
        
        // Simulating the hierarchy for testing
        subTaxon.Parent = rootTaxon;
        rootTaxon.UpdatePermalink(taxonomy.Name); // "clothing/men"

        // Act
        subTaxon.UpdatePermalink(taxonomy.Name);

        // Assert
        rootTaxon.Permalink.Should().Be("clothing/men");
        subTaxon.Permalink.Should().Be("clothing/men/shoes");
    }
}
