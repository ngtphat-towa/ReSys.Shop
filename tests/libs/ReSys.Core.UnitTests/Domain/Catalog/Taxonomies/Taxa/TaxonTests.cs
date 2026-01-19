using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.UnitTests.Domain.Catalog.Taxonomies.Taxa;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "Taxon")]
public class TaxonTests
{
    private readonly Guid _taxonomyId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize taxon")]
    public void Create_Should_InitializeTaxon()
    {
        // Act
        var result = Taxon.Create(_taxonomyId, "Shirts", slug: "custom-shirts");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TaxonomyId.Should().Be(_taxonomyId);
        result.Value.Name.Should().Be("Shirts");
        result.Value.Slug.Should().Be("custom-shirts");
        result.Value.DomainEvents.Should().ContainSingle(e => e is TaxonEvents.TaxonCreated);
    }

    [Fact(DisplayName = "Update: Should change properties and raise event")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var taxon = Taxon.Create(_taxonomyId, "Old Name").Value;

        // Act
        var result = taxon.Update("New Name", "New Presentation", "New Desc", automatic: true);

        // Assert
        result.IsError.Should().BeFalse();
        taxon.Name.Should().Be("New Name");
        taxon.Presentation.Should().Be("New Presentation");
        taxon.Description.Should().Be("New Desc");
        taxon.Automatic.Should().BeTrue();
        taxon.MarkedForRegenerateProducts.Should().BeTrue();
        taxon.DomainEvents.Should().Contain(e => e is TaxonEvents.TaxonUpdated);
    }

    [Fact(DisplayName = "AddRule: Should add new rule and mark for regeneration")]
    public void AddRule_Should_AddNewRule()
    {
        // Arrange
        var taxon = Taxon.Create(_taxonomyId, "Automatic Taxon", automatic: true).Value;
        taxon.ClearDomainEvents();

        // Act
        var result = taxon.AddRule("product_name", "shirt", "contains");

        // Assert
        result.IsError.Should().BeFalse();
        taxon.TaxonRules.Should().HaveCount(1);
        taxon.MarkedForRegenerateProducts.Should().BeTrue();
        taxon.DomainEvents.Should().ContainSingle(e => e is TaxonRuleEvents.TaxonRuleAdded);
    }

    [Fact(DisplayName = "AddRule: Should fail if rule is duplicate")]
    public void AddRule_ShouldFail_IfDuplicate()
    {
        // Arrange
        var taxon = Taxon.Create(_taxonomyId, "Automatic Taxon", automatic: true).Value;
        taxon.AddRule("product_name", "shirt", "contains");

        // Act
        var result = taxon.AddRule("product_name", "shirt", "contains");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonRuleErrors.Duplicate);
    }

    [Fact(DisplayName = "RemoveRule: Should remove existing rule")]
    public void RemoveRule_Should_RemoveRule()
    {
        // Arrange
        var taxon = Taxon.Create(_taxonomyId, "Automatic Taxon", automatic: true).Value;
        var rule = taxon.AddRule("product_name", "shirt", "contains").Value;

        // Act
        var result = taxon.RemoveRule(rule.Id);

        // Assert
        result.IsError.Should().BeFalse();
        taxon.TaxonRules.Should().BeEmpty();
        taxon.DomainEvents.Should().Contain(e => e is TaxonRuleEvents.TaxonRuleRemoved);
    }

    [Fact(DisplayName = "SetParent: Should fail for root taxon")]
    public void SetParent_ShouldFail_ForRootTaxon()
    {
        // Arrange
        var rootTaxon = Taxon.Create(_taxonomyId, "Root").Value; // ParentId is null

        // Act
        var result = rootTaxon.SetParent(Guid.NewGuid());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonErrors.RootLock);
    }

    [Fact(DisplayName = "Delete: Should fail if taxon has children")]
    public void Delete_ShouldFail_IfHasChildren()
    {
        // Arrange
        var taxon = Taxon.Create(_taxonomyId, "Parent", Guid.NewGuid()).Value;
        taxon.Children.Add(Taxon.Create(_taxonomyId, "Child", taxon.Id).Value);

        // Act
        var result = taxon.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonErrors.HasChildren);
    }
}