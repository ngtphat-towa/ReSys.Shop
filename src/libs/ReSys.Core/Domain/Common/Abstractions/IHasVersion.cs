namespace ReSys.Core.Domain.Common.Abstractions;

/// <summary>
/// Defines an entity that supports optimistic concurrency via a version property.
/// </summary>
public interface IHasVersion
{
    /// <summary>
    /// Gets or sets the version of the entity.
    /// </summary>
    long Version { get; set; }
}