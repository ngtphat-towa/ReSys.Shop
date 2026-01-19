using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Roles;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.Roles.Claims;

/// <summary>
/// Represents a claim assigned to a role, defining permissions for all users in that role.
/// Inherits from IdentityRoleClaim but implements project-specific metadata and guards
/// to ensure ERP-level auditability and data integrity.
/// </summary>
public sealed class RoleClaim : IdentityRoleClaim<string>, IHasAssignable
{
    #region Properties
    /// <summary>Timestamp when this claim was assigned to the role.</summary>
    public DateTimeOffset? AssignedAt { get; set; }
    
    /// <summary>Identifier of the administrator or system process that assigned this claim.</summary>
    public string? AssignedBy { get; set; }
    
    /// <summary>The target of the assignment (usually the RoleId).</summary>
    public string? AssignedTo { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    #endregion

    /// <summary>
    /// Required for EF Core and Identity instantiation.
    /// </summary>
    public RoleClaim() { }

    #region Factory Methods
    /// <summary>
    /// Factory for creating a role-specific claim with audit metadata.
    /// </summary>
    public static ErrorOr<RoleClaim> Create(string roleId, string claimType, string? claimValue = null, string? assignedBy = null)
    {
        // Guard: Required identification
        if (string.IsNullOrWhiteSpace(roleId))
            return RoleClaimErrors.RoleIdRequired;
        
        if (string.IsNullOrWhiteSpace(claimType))
            return RoleClaimErrors.ClaimTypeRequired;

        // Guard: Business constraints validation
        if (claimType.Length > RoleClaimConstraints.ClaimTypeMaxLength)
            return RoleClaimErrors.ClaimTypeTooLong(RoleClaimConstraints.ClaimTypeMaxLength);

        if (claimValue?.Length > RoleClaimConstraints.ClaimValueMaxLength)
            return RoleClaimErrors.ClaimValueTooLong(RoleClaimConstraints.ClaimValueMaxLength);
            
        if (assignedBy?.Length > RoleClaimConstraints.AssignedByMaxLength)
            return RoleClaimErrors.AssignedByTooLong(RoleClaimConstraints.AssignedByMaxLength);

        return new RoleClaim
        {
            RoleId = roleId,
            ClaimType = claimType.Trim(),
            ClaimValue = claimValue?.Trim(),
            AssignedTo = roleId,
            AssignedBy = assignedBy?.Trim(),
            AssignedAt = DateTimeOffset.UtcNow
        };
    }
    #endregion
}