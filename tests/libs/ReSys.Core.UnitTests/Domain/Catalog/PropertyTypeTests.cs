using FluentAssertions;
using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class PropertyTypeTests
{
    [Fact(DisplayName = "Create should support different property kinds")]
    public void Create_ShouldSupport_PropertyKinds()
    {
        // Act
        var result = PropertyType.Create("Material", kind: PropertyType.PropertyKind.String);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Kind.Should().Be(PropertyType.PropertyKind.String);
    }

    [Fact(DisplayName = "Update should modify kind and raise event")]
    public void Update_ShouldModifyKind_AndRaiseEvent()
    {
        // Arrange
        var prop = PropertyType.Create("Weight").Value;
        prop.ClearDomainEvents();

        // Act
        var result = prop.Update("New Name", "Pres", PropertyType.PropertyKind.Float, 0, false);

        // Assert
        result.IsError.Should().BeFalse();
        prop.Kind.Should().Be(PropertyType.PropertyKind.Float);
        prop.DomainEvents.Should().ContainSingle(e => e is PropertyTypeEvents.PropertyTypeUpdated);
    }
}
