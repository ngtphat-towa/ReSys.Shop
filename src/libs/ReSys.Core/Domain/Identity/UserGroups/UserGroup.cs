using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups;

public sealed class UserGroup : Aggregate, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public ICollection<UserGroupMembership> Memberships { get; set; } = new List<UserGroupMembership>();

    private UserGroup() { }

    public static ErrorOr<UserGroup> Create(
        string name,
        string code,
        string? description = null,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return UserGroupErrors.NameRequired;
        if (string.IsNullOrWhiteSpace(code)) return UserGroupErrors.CodeRequired;

        var group = new UserGroup
        {
            Name = name.Trim(),
            Code = code.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            IsDefault = isDefault,
            IsActive = true
        };

        group.RaiseDomainEvent(new UserGroupEvents.UserGroupCreated(group));
        return group;
    }
}