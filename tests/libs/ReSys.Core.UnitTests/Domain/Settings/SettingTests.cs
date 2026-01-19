using ReSys.Core.Domain.Settings;

namespace ReSys.Core.UnitTests.Domain.Settings;

[Trait("Category", "Unit")]
[Trait("Module", "Settings")]
[Trait("Domain", "Setting")]
public class SettingTests
{
    [Fact(DisplayName = "Create: Should successfully initialize setting")]
    public void Create_Should_InitializeSetting()
    {
        // Act
        var result = Setting.Create("shop.name", "ReSys Shop", "Store name", "ReSys");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Key.Should().Be("shop.name");
        result.Value.Value.Should().Be("ReSys Shop");
        result.Value.DefaultValue.Should().Be("ReSys");
        result.Value.DomainEvents.Should().ContainSingle(e => e is SettingEvents.SettingCreated);
    }

    [Fact(DisplayName = "Create: Should fail if key is empty")]
    public void Create_ShouldFail_IfKeyEmpty()
    {
        // Act
        var result = Setting.Create("", "Value");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SettingErrors.KeyRequired);
    }

    [Fact(DisplayName = "UpdateValue: Should change current value")]
    public void UpdateValue_Should_ChangeValue()
    {
        // Arrange
        var setting = Setting.Create("key", "old").Value;
        setting.ClearDomainEvents();

        // Act
        var result = setting.UpdateValue("new");

        // Assert
        result.IsError.Should().BeFalse();
        setting.Value.Should().Be("new");
        setting.DomainEvents.Should().ContainSingle(e => e is SettingEvents.SettingUpdated);
    }

    [Fact(DisplayName = "UpdateValue: Should fail if value is empty")]
    public void UpdateValue_ShouldFail_IfValueEmpty()
    {
        // Arrange
        var setting = Setting.Create("key", "old").Value;

        // Act
        var result = setting.UpdateValue("");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SettingErrors.ValueRequired);
    }

    [Fact(DisplayName = "UpdateDetails: Should update metadata and description")]
    public void UpdateDetails_Should_UpdateInfo()
    {
        // Arrange
        var setting = Setting.Create("key", "val").Value;
        var meta = new Dictionary<string, object?> { ["Group"] = "UI" };

        // Act
        var result = setting.UpdateDetails("New Description", "New Default", meta);

        // Assert
        result.IsError.Should().BeFalse();
        setting.Description.Should().Be("New Description");
        setting.DefaultValue.Should().Be("New Default");
        setting.PublicMetadata.Should().ContainKey("Group");
    }

    [Fact(DisplayName = "ResetToDefault: Should revert value if default exists")]
    public void ResetToDefault_Should_RevertValue()
    {
        // Arrange
        var setting = Setting.Create("key", "current", defaultValue: "default").Value;

        // Act
        setting.ResetToDefault();

        // Assert
        setting.Value.Should().Be("default");
    }
}
