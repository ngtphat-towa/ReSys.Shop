using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Settings;

public sealed class Setting : Aggregate, IHasMetadata
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
    public ConfigurationValueType ValueType { get; set; } = ConfigurationValueType.String;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public Setting() { }

    public static ErrorOr<Setting> Create(
        string key,
        string value,
        string? description = null,
        string? defaultValue = null,
        ConfigurationValueType valueType = ConfigurationValueType.String)
    {
        if (string.IsNullOrWhiteSpace(key))
            return SettingErrors.KeyRequired;

        var setting = new Setting
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            Value = value,
            Description = description?.Trim(),
            DefaultValue = defaultValue,
            ValueType = valueType
        };

        setting.RaiseDomainEvent(new SettingEvents.SettingCreated(setting));
        return setting;
    }

    public ErrorOr<Success> UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return SettingErrors.ValueRequired;

        Value = value;
        RaiseDomainEvent(new SettingEvents.SettingUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Success> UpdateDetails(
        string? description = null,
        string? defaultValue = null,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        Description = description?.Trim() ?? Description;
        DefaultValue = defaultValue ?? DefaultValue;

        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);

        RaiseDomainEvent(new SettingEvents.SettingUpdated(this));
        return Result.Success;
    }

    public void ResetToDefault()
    {
        if (DefaultValue != null)
        {
            Value = DefaultValue;
            RaiseDomainEvent(new SettingEvents.SettingUpdated(this));
        }
    }
}
