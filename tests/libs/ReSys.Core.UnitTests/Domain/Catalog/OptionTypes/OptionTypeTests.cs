using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Domain.Catalog.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Domain", "OptionType")]
public class OptionTypeTests
{
    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Act
        var result = OptionType.Create("Color", "Select Color", 1, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Color");
        result.Value.Presentation.Should().Be("Select Color");
        result.Value.Position.Should().Be(1);
        result.Value.Filterable.Should().BeTrue();
        result.Value.DomainEvents.Should().ContainSingle(e => e is OptionTypeEvents.OptionTypeCreated);
    }

    [Fact(DisplayName = "Create should fail when name is empty")]
    public void Create_ShouldFail_WhenNameIsEmpty()
    {
        var result = OptionType.Create("");
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.NameRequired);
    }

    [Fact(DisplayName = "Create should fail when name is too long")]
    public void Create_ShouldFail_WhenNameIsTooLong()
    {
        var result = OptionType.Create(new string('A', OptionTypeConstraints.NameMaxLength + 1));
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.NameTooLong);
    }

    [Fact(DisplayName = "Create should fail when position is negative")]
    public void Create_ShouldFail_WhenPositionIsNegative()
    {
        var result = OptionType.Create("Valid", "Valid", -1);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.InvalidPosition);
    }

    [Fact(DisplayName = "Update should succeed with valid data")]
    public void Update_ShouldSucceed_WithValidData()
    {
        // Arrange
        var optionType = OptionType.Create("Old").Value;
        optionType.ClearDomainEvents();

        // Act
        var result = optionType.Update("New", "New Presentation", 5, true);

        // Assert
        result.IsError.Should().BeFalse();
        optionType.Name.Should().Be("New");
        optionType.Presentation.Should().Be("New Presentation");
        optionType.Position.Should().Be(5);
        optionType.Filterable.Should().BeTrue();
        optionType.DomainEvents.Should().ContainSingle(e => e is OptionTypeEvents.OptionTypeUpdated);
    }

    [Fact(DisplayName = "Update should fail when name is empty")]
    public void Update_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var optionType = OptionType.Create("Valid").Value;

        // Act
        var result = optionType.Update("", "Valid", 0, false);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.NameRequired);
    }

    [Fact(DisplayName = "Delete should succeed when no values exist")]
    public void Delete_ShouldSucceed_WhenNoValuesExist()
    {
        // Arrange
        var optionType = OptionType.Create("Material").Value;
        optionType.ClearDomainEvents();

        // Act
        var result = optionType.Delete();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        optionType.DomainEvents.Should().ContainSingle(e => e is OptionTypeEvents.OptionTypeDeleted);
    }

    [Fact(DisplayName = "Delete should fail when values exist")]
    public void Delete_ShouldFail_WhenValuesExist()
    {
        // Arrange
        var optionType = OptionType.Create("Material").Value;
        optionType.AddValue("Cotton");

        // Act
        var result = optionType.Delete();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(OptionTypeErrors.CannotDeleteWithValues);
    }

    [Fact(DisplayName = "Metadata should be manageable")]
    public void Metadata_ShouldBe_Manageable()
    {
        // Arrange
        var optionType = OptionType.Create("Material").Value;

        // Act
        optionType.PublicMetadata["Theme"] = "Dark";
        optionType.PrivateMetadata["InternalCode"] = "MT-001";

        // Assert
        optionType.PublicMetadata.Should().ContainKey("Theme").WhoseValue.Should().Be("Dark");
        optionType.PrivateMetadata.Should().ContainKey("InternalCode").WhoseValue.Should().Be("MT-001");
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
        var result = optionType.AddValue("red");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(OptionValueErrors.NameAlreadyExists("red").Code);
    }

    [Fact(DisplayName = "ReorderValues should update positions correctly")]
    public void ReorderValues_ShouldUpdate_Positions()
    {
        // Arrange
        var optionType = OptionType.Create("Size").Value;
        var v1 = optionType.AddValue("Small").Value;
        var v2 = optionType.AddValue("Medium").Value;

        // Act
        var result = optionType.ReorderValues([(v1.Id, 10), (v2.Id, 20)]);

        // Assert
        result.IsError.Should().BeFalse();
        v1.Position.Should().Be(10);
        v2.Position.Should().Be(20);
    }

    [Fact(DisplayName = "ReorderValues should ignore non-existent IDs")]
    public void ReorderValues_ShouldIgnore_NonExistentIds()
    {
        // Arrange
        var optionType = OptionType.Create("Size").Value;
        var v1 = optionType.AddValue("Small").Value;

        // Act
        var result = optionType.ReorderValues([(v1.Id, 10), (Guid.NewGuid(), 20)]);

        // Assert
        result.IsError.Should().BeFalse();
        v1.Position.Should().Be(10);
    }
}
