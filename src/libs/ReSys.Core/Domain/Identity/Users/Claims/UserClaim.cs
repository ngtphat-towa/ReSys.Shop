using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Claims;

/// <summary>
/// Represents a specific claim assigned to a user for fine-grained authorization.
/// Inherits from IdentityUserClaim but implements project-specific metadata and guards
/// to ensure ERP-level auditability and data integrity.
/// </summary>
public class UserClaim : IdentityUserClaim<string>, IHasAssignable
{
    #region Properties
    /// <summary>Timestamp when this claim was assigned to the user.</summary>
    public DateTimeOffset? AssignedAt { get; set; }

    /// <summary>Identifier of the administrator or system process that assigned this claim.</summary>
    public string? AssignedBy { get; set; }

    /// <summary>The target of the assignment (usually the UserId).</summary>
    public string? AssignedTo { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    #endregion

    /// <summary>
    /// Required for EF Core and Identity instantiation.
    /// </summary>
    public UserClaim() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a user-specific claim with audit metadata.
    /// </summary>
    public static ErrorOr<UserClaim> Create(string userId, string claimType, string? claimValue = null, string? assignedBy = null)
    {
        // Guard: Required identification
        if (string.IsNullOrWhiteSpace(userId))
            return UserClaimErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(claimType))
            return UserClaimErrors.ClaimTypeRequired;

        // Guard: Business constraints validation
        if (claimType.Length > UserClaimConstraints.ClaimTypeMaxLength)
            return UserClaimErrors.ClaimTypeTooLong(UserClaimConstraints.ClaimTypeMaxLength);

        if (claimValue?.Length > UserClaimConstraints.ClaimValueMaxLength)
            return UserClaimErrors.ClaimValueTooLong(UserClaimConstraints.ClaimValueMaxLength);

        if (assignedBy?.Length > UserClaimConstraints.AssignedByMaxLength)
            return UserClaimErrors.AssignedByTooLong(UserClaimConstraints.AssignedByMaxLength);

        return new UserClaim
        {
            UserId = userId,
            ClaimType = claimType.Trim(),
            ClaimValue = claimValue?.Trim(),
            AssignedTo = userId,
            AssignedBy = assignedBy?.Trim(),
            AssignedAt = DateTimeOffset.UtcNow
        };
    }
    #endregion
}