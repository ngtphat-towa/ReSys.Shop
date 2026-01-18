using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.OptionTypes.OptionValues;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "OptionValue")]
public class OptionValueTests
{
    [Fact(DisplayName = "Update should succeed with valid data")]
    public void Update_ShouldSucceed_WithValidData()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        var ov = ot.AddValue("Small", "S").Value;
        ov.ClearDomainEvents();

        // Act
        var result = ov.Update("ExtraSmall", "XS", 5);

        // Assert
        result.IsError.Should().BeFalse();
        ov.Name.Should().Be("ExtraSmall");
        ov.Presentation.Should().Be("XS");
        ov.Position.Should().Be(5);
        ov.DomainEvents.Should().ContainSingle(e => e is OptionValueEvents.OptionValueUpdated);
    }

    [Fact(DisplayName = "Update should fail when name is empty")]
    public void Update_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        var ov = ot.AddValue("Small").Value;

        // Act
        var result = ov.Update("", "Valid", 0);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionValueErrors.NameRequired);
    }

    [Fact(DisplayName = "Update should fail when presentation is empty")]
    public void Update_ShouldFail_WhenPresentationIsEmpty()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        var ov = ot.AddValue("Small").Value;

        // Act
        var result = ov.Update("Valid", "", 0);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionValueErrors.PresentationRequired);
    }

    [Fact(DisplayName = "Delete should raise deleted event")]
    public void Delete_Should_RaiseDeletedEvent()
    {
        // Arrange
        var ot = OptionType.Create("Size").Value;
        var ov = ot.AddValue("Small").Value;
        ov.ClearDomainEvents();

        // Act
        var result = ov.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        ov.DomainEvents.Should().ContainSingle(e => e is OptionValueEvents.OptionValueDeleted);
    }
}