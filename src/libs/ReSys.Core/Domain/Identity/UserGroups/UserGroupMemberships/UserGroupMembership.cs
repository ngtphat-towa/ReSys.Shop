using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;

/// <summary>
/// Represents the relationship between a User and a UserGroup.
/// Captures audit data regarding when and by whom the assignment was made.
/// </summary>
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

    public UserGroupMembership() { }

    /// <summary>
    /// Factory to ensure memberships are managed through aggregate roots.
    /// </summary>
    public static ErrorOr<UserGroupMembership> Create(string userId, Guid groupId, string? assignedBy = null, bool isPrimary = false)
    {
        // Guard: Required identification
        if (string.IsNullOrWhiteSpace(userId)) 
            return UserGroupMembershipErrors.UserIdRequired;

        // Guard: Metadata constraints
        if (assignedBy?.Length > UserGroupMembershipConstraints.AssignedByMaxLength)
            return UserGroupMembershipErrors.AssignedByTooLong(UserGroupMembershipConstraints.AssignedByMaxLength);

        return new UserGroupMembership
        {
            UserId = userId,
            UserGroupId = groupId,
            JoinedAt = DateTimeOffset.UtcNow,
            AssignedBy = assignedBy?.Trim(),
            IsPrimary = isPrimary
        };
    }

    /// <summary>
    /// Changes the primary status of this membership.
    /// </summary>
    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}