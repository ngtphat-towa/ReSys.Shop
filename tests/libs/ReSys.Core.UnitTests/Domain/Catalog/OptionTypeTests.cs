using ReSys.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Core.UnitTests.Domain.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class OptionTypeTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = OptionType.Create("Color", "Select Color");

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Color");
        result.Value.DomainEvents.Should().ContainSingle(e => e is OptionTypeEvents.OptionTypeCreated);
    }

    [Fact(DisplayName = "AddValue should assign increasing positions")]
    public void AddValue_ShouldAssign_IncreasingPositions()
    {
        // Arrange
        var optionType = OptionType.Create("Size").Value;

        // Act
        var v1 = optionType.AddValue("Small").Value;
        var v2 = optionType.AddValue("Medium").Value;

        // Assert
        v1.Position.Should().Be(0);
        v2.Position.Should().Be(1);
        optionType.OptionValues.Should().HaveCount(2);
    }

    [Fact(DisplayName = "AddValue should prevent duplicate names")]
    public void AddValue_ShouldPrevent_DuplicateNames()
    {
        // Arrange
        var optionType = OptionType.Create("Color").Value;
        optionType.AddValue("Red");

        // Act
        var result = optionType.AddValue("red"); // Case-insensitive check

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("OptionValue.NameAlreadyExists");
    }
}