using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;

public sealed class UserGroupMembership : Entity
{
    public string UserId { get; set; } = string.Empty;
    public Guid UserGroupId { get; set; }

    // Relationship Meta-data
    public DateTimeOffset JoinedAt { get; set; }
    public string? AssignedBy { get; set; }
    public bool IsPrimary { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public UserGroup Group { get; set; } = null!;

    private UserGroupMembership() { }

    internal static UserGroupMembership Create(string userId, Guid groupId, string? assignedBy = null, bool isPrimary = false)
    {
        return new UserGroupMembership
        {
            UserId = userId,
            UserGroupId = groupId,
            JoinedAt = DateTimeOffset.UtcNow,
            AssignedBy = assignedBy,
            IsPrimary = isPrimary
        };
    }
}
