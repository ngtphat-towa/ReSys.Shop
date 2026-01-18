using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.Taxonomies.Taxa.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "TaxonRule")]
public class TaxonRuleTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var result = TaxonRule.Create(Guid.NewGuid(), "product_name", "Cotton", "contains");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be("product_name");
        result.Value.Value.Should().Be("Cotton");
        result.Value.MatchPolicy.Should().Be("contains");
    }

    [Fact(DisplayName = "Create should fail when value is empty")]
    public void Create_ShouldFail_WhenValueIsEmpty()
    {
        var result = TaxonRule.Create(Guid.NewGuid(), "product_name", "");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonRuleErrors.ValueRequired);
    }

    [Fact(DisplayName = "Create should fail when type is invalid")]
    public void Create_ShouldFail_WhenTypeIsInvalid()
    {
        var result = TaxonRule.Create(Guid.NewGuid(), "invalid_type", "Value");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonRuleErrors.InvalidType);
    }

    [Fact(DisplayName = "Create should fail when property name is missing for product_property type")]
    public void Create_ShouldFail_WhenPropertyNameMissing()
    {
        var result = TaxonRule.Create(Guid.NewGuid(), "product_property", "Value");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonRuleErrors.PropertyNameRequired);
    }

    [Fact(DisplayName = "Update should succeed with valid data")]
    public void Update_ShouldSucceed_WithValidData()
    {
        // Arrange
        var rule = TaxonRule.Create(Guid.NewGuid(), "product_name", "Old").Value;

        // Act
        var result = rule.Update(value: "New", matchPolicy: "starts_with");

        // Assert
        result.IsError.Should().BeFalse();
        rule.Value.Should().Be("New");
        rule.MatchPolicy.Should().Be("starts_with");
    }
}
