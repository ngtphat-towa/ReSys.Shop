using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Taxonomy")]
public class TaxonomyTests
{
    [Fact(DisplayName = "Create should succeed and automatically create a root taxon")]
    public void Create_ShouldSucceed_AndCreateRoot()
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

    [Fact(DisplayName = "Create should fail when position is negative")]
    public void Create_ShouldFail_WhenPositionIsNegative()
    {
        var result = Taxonomy.Create("Valid", "Valid", -1);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonomyErrors.InvalidPosition);
    }

    [Fact(DisplayName = "Update taxonomy should synchronize its root taxon")]
    public void Update_Should_SyncRootTaxon()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Old").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.Update("New", "New Pres", 5);

        // Assert
        result.IsError.Should().BeFalse();
        taxonomy.Name.Should().Be("New");
        root.Name.Should().Be("New");
        root.Presentation.Should().Be("New Pres");
        root.Slug.Should().Be("new");
    }

    [Fact(DisplayName = "Delete should fail if taxonomy has more than just the root taxon")]
    public void Delete_ShouldFail_IfHasChildren()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Catalog").Value;
        taxonomy.AddTaxon("SubCategory");

        // Act
        var result = taxonomy.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonomyErrors.HasTaxons);
    }

    [Fact(DisplayName = "AddTaxon without parentId should automatically use root as parent")]
    public void AddTaxon_NoParentId_UsesRoot()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Catalog").Value;
        var root = taxonomy.RootTaxon!;

        // Act
        var result = taxonomy.AddTaxon("Clothing");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ParentId.Should().Be(root.Id);
    }

    [Fact(DisplayName = "AddTaxon with explicit parentId should respect it")]
    public async Task AddTaxon_WithParentId_RespectsIt()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("Catalog").Value;
        var root = taxonomy.RootTaxon!;
        var sub = taxonomy.AddTaxon("Clothing").Value;

        // Act
        var result = taxonomy.AddTaxon("Shirts", sub.Id);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ParentId.Should().Be(sub.Id);
    }
}
