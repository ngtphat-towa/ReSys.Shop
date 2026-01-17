using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Shared.Extensions;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonomyTests
{
    [Fact(DisplayName = "Create should succeed and automatically create a root taxon")]
    public void Create_ShouldSucceed_AndCreateRootTaxon()
    {
        // Act
        var result = Taxonomy.Create("Categories", "Product Categories");

        // Assert
        result.IsError.Should().BeFalse();
        var taxonomy = result.Value;
        taxonomy.Name.Should().Be("Categories");
        
        // Single root taxon should exist
        taxonomy.Taxons.Should().HaveCount(1);
        var root = taxonomy.RootTaxon;
        root.Should().NotBeNull();
        root!.Name.Should().Be(taxonomy.Name);
        root.ParentId.Should().BeNull();
        root.Permalink.Should().Be(taxonomy.Name.ToSlug());
    }

    [Fact(DisplayName = "Update taxonomy should synchronize its root taxon")]
    public void Update_ShouldSynchronizeRootTaxon()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Old Name", "Old Presentation").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.Update("New Name", "New Presentation", 10);

        // Assert
        result.IsError.Should().BeFalse();
        taxonomy.Name.Should().Be("New Name");
        taxonomy.Position.Should().Be(10);

        root.Name.Should().Be("New Name");
        root.Presentation.Should().Be("New Presentation");
        root.Slug.Should().Be("new-name");
        root.Permalink.Should().Be("new-name");
    }

    [Fact(DisplayName = "AddTaxon without parentId should automatically use root as parent")]
    public void AddTaxon_WithoutParent_ShouldUseRootAsParent()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Electronics").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.AddTaxon("Laptops");

        // Assert
        result.IsError.Should().BeFalse();
        var laptopTaxon = result.Value;
        laptopTaxon.ParentId.Should().Be(root.Id);
        taxonomy.Taxons.Should().HaveCount(2);
    }

    [Fact(DisplayName = "AddTaxon with explicit parentId should respect it")]
    public void AddTaxon_WithExplicitParent_ShouldRespectIt()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Clothing").Value;
        var root = taxonomy.RootTaxon!;
        var men = taxonomy.AddTaxon("Men", root.Id).Value;

        // Act
        var result = taxonomy.AddTaxon("Shoes", men.Id);

        // Assert
        result.IsError.Should().BeFalse();
        var shoes = result.Value;
        shoes.ParentId.Should().Be(men.Id);
        taxonomy.Taxons.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Delete should fail if taxonomy has more than just the root taxon")]
    public void Delete_ShouldFail_IfHasChildTaxons()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Books").Value;
        taxonomy.AddTaxon("Fiction");

        // Act
        var result = taxonomy.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxonomy.HasTaxons");
    }

    [Fact(DisplayName = "Taxon UpdatePermalink should build deep paths correctly")]
    public void Taxon_UpdatePermalink_ShouldBuildDeepPaths()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Tech").Value;
        var root = taxonomy.RootTaxon!;
        
        var computers = taxonomy.AddTaxon("Computers").Value;
        computers.Parent = root;
        computers.UpdatePermalink(taxonomy.Name);

        var laptops = Taxon.Create(taxonomy.Id, "Laptops", parentId: computers.Id).Value;
        laptops.Parent = computers;

        // Act
        laptops.UpdatePermalink(taxonomy.Name);

        // Assert
        root.Permalink.Should().Be("tech");
        computers.Permalink.Should().Be("tech/computers");
        laptops.Permalink.Should().Be("tech/computers/laptops");
        laptops.PrettyName.Should().Be("Tech -> Computers -> Laptops");
    }
}