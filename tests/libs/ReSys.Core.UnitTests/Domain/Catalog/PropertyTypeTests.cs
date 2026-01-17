using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Domain")]
[Trait("Module", "Catalog")]
[Trait("Feature", "PropertyTypes")]
public class PropertyTypeTests
{
    [Fact(DisplayName = "Create: Should create property type with valid inputs")]
    public void Create_ValidInputs_ShouldSucceed()
    {
        var result = PropertyType.Create("Material", "Fabric", PropertyKind.String, 1, true);

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Material");
        result.Value.Kind.Should().Be(PropertyKind.String);
    }

    [Fact(DisplayName = "Update: Should update property type state correctly")]
    public void Update_ValidInputs_ShouldSucceed()
    {
        var pt = PropertyType.Create("Weight").Value;
        var result = pt.Update("Weight", "Gross Weight", PropertyKind.Float, 5, true);

        result.IsError.Should().BeFalse();
        pt.Presentation.Should().Be("Gross Weight");
        pt.Kind.Should().Be(PropertyKind.Float);
    }
}