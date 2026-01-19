using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.UnitTests.Domain.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "OptionValue")]
public class OptionValueTests
{
    private readonly Guid _optionTypeId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize option value and normalize inputs")]
    public void Create_Should_InitializeValue()
    {
        // Act
        var result = OptionValue.Create(_optionTypeId, " red ", " Bright Red ", 10);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.OptionTypeId.Should().Be(_optionTypeId);
        result.Value.Name.Should().Be("red");
        result.Value.Presentation.Should().Be("Bright Red");
        result.Value.Position.Should().Be(10);
        result.Value.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Update: Should change properties and raise event")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var value = OptionValue.Create(_optionTypeId, "Old").Value;
        value.ClearDomainEvents();

        // Act
        var result = value.Update("NewName", "New Presentation", 5);

        // Assert
        result.IsError.Should().BeFalse();
        value.Name.Should().Be("NewName");
        value.Presentation.Should().Be("New Presentation");
        value.Position.Should().Be(5);
        value.DomainEvents.Should().ContainSingle(e => e is OptionValueEvents.OptionValueUpdated);
    }

    [Fact(DisplayName = "Update: Should fail if name is missing")]
    public void Update_ShouldFail_IfNameEmpty()
    {
        // Arrange
        var value = OptionValue.Create(_optionTypeId, "Valid").Value;

        // Act
        var result = value.Update("", "Presentation", 0);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("OptionValue.NameRequired");
    }

    [Fact(DisplayName = "Delete: Should raise deleted event")]
    public void Delete_Should_RaiseEvent()
    {
        // Arrange
        var value = OptionValue.Create(_optionTypeId, "DeleteMe").Value;
        value.ClearDomainEvents();

        // Act
        var result = value.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        value.DomainEvents.Should().ContainSingle(e => e is OptionValueEvents.OptionValueDeleted);
    }
}
