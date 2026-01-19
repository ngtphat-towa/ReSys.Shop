using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.UnitTests.Domain.Catalog.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Taxonomy")]
public class TaxonomyTests
{
    [Fact(DisplayName = "Create: Should successfully initialize taxonomy and root taxon")]
    public void Create_Should_InitializeTaxonomyAndRoot()
    {
        // Act
        var result = Taxonomy.Create("Categories", "Product Categories", 1);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Categories");
        result.Value.Presentation.Should().Be("Product Categories");
        result.Value.Position.Should().Be(1);
        
        result.Value.RootTaxon.Should().NotBeNull();
        result.Value.RootTaxon!.Name.Should().Be("Categories");
        result.Value.RootTaxon.ParentId.Should().BeNull();
        
        result.Value.DomainEvents.Should().ContainSingle(e => e is TaxonomyEvents.TaxonomyCreated);
    }

    [Fact(DisplayName = "Update: Should synchronize name changes with root taxon")]
    public void Update_Should_SyncRootTaxon()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Old Name").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.Update("New Name", "New Presentation", 5);

        // Assert
        result.IsError.Should().BeFalse();
        taxonomy.Name.Should().Be("New Name");
        root.Name.Should().Be("New Name");
        root.Presentation.Should().Be("New Presentation");
        root.Slug.Should().Be("new-name");
    }

    [Fact(DisplayName = "AddTaxon: Should assign root as parent if none specified")]
    public void AddTaxon_Should_DefaultToRootParent()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Catalog").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.AddTaxon("Electronics");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ParentId.Should().Be(root.Id);
        taxonomy.Taxons.Should().Contain(result.Value);
    }

    [Fact(DisplayName = "Delete: Should fail if non-root taxons exist")]
    public void Delete_ShouldFail_IfHasChildren()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Catalog").Value;
        taxonomy.AddTaxon("Sub");

        // Act
        var result = taxonomy.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonomyErrors.HasTaxons);
    }

    [Fact(DisplayName = "Create: Should fail if position is negative")]
    public void Create_ShouldFail_IfPositionNegative()
    {
        // Act
        var result = Taxonomy.Create("Test", "Test", -1);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonomyErrors.InvalidPosition);
    }
}
