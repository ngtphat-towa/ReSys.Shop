using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups;

/// <summary>
/// Represents a logical collection of users for categorization and mass-authorization.
/// Aggregate Root.
/// </summary>
public sealed class UserGroup : Aggregate, IHasMetadata, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemGroup { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public ICollection<UserGroupMembership> Memberships { get; set; } = new List<UserGroupMembership>();

    public UserGroup() { }

    /// <summary>
    /// Factory for creating a new user group.
    /// </summary>
    public static ErrorOr<UserGroup> Create(
        string name, 
        string code, 
        string? description = null, 
        bool isSystemGroup = false,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("UserGroup.NameRequired", "Group name is required.");
        if (string.IsNullOrWhiteSpace(code)) return Error.Validation("UserGroup.CodeRequired", "Group code is required.");

        var group = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            IsSystemGroup = isSystemGroup,
            IsDefault = isDefault,
            IsActive = true
        };

        group.RaiseDomainEvent(new UserGroupEvents.GroupCreated(group));
        return group;
    }

    /// <summary>
    /// Updates group metadata.
    /// </summary>
    public ErrorOr<Success> Update(string name, string? description, bool isDefault = false)
    {
        if (IsSystemGroup) return Error.Validation("UserGroup.SystemGroupProtected", "System groups cannot be renamed.");
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("UserGroup.NameRequired", "Group name is required.");

        Name = name.Trim();
        Description = description?.Trim();
        IsDefault = isDefault;

        RaiseDomainEvent(new UserGroupEvents.GroupUpdated(this));
        return Result.Success;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public ErrorOr<Deleted> Delete()
    {
        if (IsSystemGroup) return Error.Validation("UserGroup.SystemGroupProtected", "System groups cannot be deleted.");
        
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new UserGroupEvents.GroupDeleted(Id, Name));
        return Result.Deleted;
    }
}
