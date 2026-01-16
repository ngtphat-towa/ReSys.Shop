namespace ReSys.Core.Domain.Settings;

/// <summary>
/// Defines the possible data types for a configuration setting's value.
/// </summary>
public enum ConfigurationValueType
{
    String,
    Boolean,
    Integer,
    Decimal,
    Guid,
    Json,
    DateTime
}
