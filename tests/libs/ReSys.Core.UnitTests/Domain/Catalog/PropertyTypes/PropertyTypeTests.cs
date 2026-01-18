using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.PropertyTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "PropertyType")]
public class PropertyTypeTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var result = PropertyType.Create("Brand", "Product Brand", PropertyKind.String, 1, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Brand");
        result.Value.Presentation.Should().Be("Product Brand");
        result.Value.Kind.Should().Be(PropertyKind.String);
        result.Value.Position.Should().Be(1);
        result.Value.Filterable.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is PropertyTypeEvents.PropertyTypeCreated);
    }

    [Fact(DisplayName = "Create should fail when name is empty")]
    public void Create_ShouldFail_WhenNameIsEmpty()
    {
        var result = PropertyType.Create("");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PropertyTypeErrors.NameRequired);
    }

    [Fact(DisplayName = "Create should fail when name is too long")]
    public void Create_ShouldFail_WhenNameIsTooLong()
    {
        var result = PropertyType.Create(new string('A', PropertyTypeConstraints.NameMaxLength + 1));
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PropertyTypeErrors.NameTooLong);
    }

    [Fact(DisplayName = "Create should fail when position is negative")]
    public void Create_ShouldFail_WhenPositionIsNegative()
    {
        var result = PropertyType.Create("Valid", "Valid", PropertyKind.String, -1);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PropertyTypeErrors.InvalidPosition);
    }

    [Fact(DisplayName = "Update should succeed with valid data")]
    public void Update_ShouldSucceed_WithValidData()
    {
        // Arrange
        var propertyType = PropertyType.Create("Old").Value;
        propertyType.ClearDomainEvents();

        // Act
        var result = propertyType.Update("New", "New Presentation", PropertyKind.Integer, 5, true);

        // Assert
        result.IsError.Should().BeFalse();
        propertyType.Name.Should().Be("New");
        propertyType.Presentation.Should().Be("New Presentation");
        propertyType.Kind.Should().Be(PropertyKind.Integer);
        propertyType.Position.Should().Be(5);
        propertyType.Filterable.Should().BeTrue();
        propertyType.DomainEvents.Should().ContainSingle(e => e is PropertyTypeEvents.PropertyTypeUpdated);
    }

    [Fact(DisplayName = "Update should fail when name is empty")]
    public void Update_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var propertyType = PropertyType.Create("Valid").Value;

        // Act
        var result = propertyType.Update("", "Valid", PropertyKind.String, 0, false);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PropertyTypeErrors.NameRequired);
    }

    [Fact(DisplayName = "Delete should raise deleted event")]
    public void Delete_Should_RaiseDeletedEvent()
    {
        // Arrange
        var propertyType = PropertyType.Create("Material").Value;
        propertyType.ClearDomainEvents();

        // Act
        var result = propertyType.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        propertyType.DomainEvents.Should().ContainSingle(e => e is PropertyTypeEvents.PropertyTypeDeleted);
    }

    [Fact(DisplayName = "Metadata should be manageable")]
    public void Metadata_ShouldBe_Manageable()
    {
        // Arrange
        var propertyType = PropertyType.Create("Material").Value;

        // Act
        propertyType.PublicMetadata["Unit"] = "cm";
        propertyType.PrivateMetadata["InternalId"] = "PT-123";

        // Assert
        propertyType.PublicMetadata.Should().ContainKey("Unit").WhoseValue.Should().Be("cm");
        propertyType.PrivateMetadata.Should().ContainKey("InternalId").WhoseValue.Should().Be("PT-123");
    }
}
