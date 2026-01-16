using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Permissions;

public sealed class AccessPermission : Entity, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private AccessPermission() { }

    public static ErrorOr<AccessPermission> Create(string name, string? displayName = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return AccessPermissionErrors.NameRequired;

        if (name.Length > AccessPermissionConstraints.NameMaxLength)
            return AccessPermissionErrors.NameTooLong;

        return new AccessPermission
        {
            Name = name.Trim().ToLowerInvariant(),
            DisplayName = displayName?.Trim() ?? name.Trim(),
            Description = description?.Trim()
        };
    }
}
