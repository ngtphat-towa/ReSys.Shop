using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ReSys.Core.Domain.Identity.Users.Roles;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.Roles;

/// <summary>
/// Represents an application role within the ASP.NET Core Identity system, serving as an aggregate root
/// for managing role-specific properties, constraints, and associated claims and user assignments.
/// </summary>
public sealed class Role : IdentityRole, IAuditable, IAggregate, IHasMetadata
{
    #region Properties
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystemRole { get; set; }
    public int Priority { get; set; }

    // Relationships
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // IAuditable implementation
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // IHasMetadata implementation
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // IAggregate implementation
    private readonly List<object> _domainEvents = [];
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);
    #endregion

    private Role() { }

    #region Factory Methods
    /// <summary>
    /// Factory method to create a new application role.
    /// </summary>
    public static ErrorOr<Role> Create(
        string name, 
        string? displayName = null, 
        string? description = null,
        int priority = 0,
        bool isDefault = false, 
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return RoleErrors.NameRequired;

        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            DisplayName = displayName?.Trim() ?? name.Trim(),
            Description = description?.Trim(),
            Priority = Math.Clamp(priority, RoleConstraints.MinPriority, RoleConstraints.MaxPriority),
            IsDefault = isDefault,
            IsSystemRole = isSystemRole,
            CreatedAt = DateTimeOffset.UtcNow
        };

        role.RaiseDomainEvent(new RoleEvents.RoleCreated(role));
        return role;
    }
    #endregion

    #region Domain Methods
    /// <summary>
    /// Updates the mutable properties of the role.
    /// </summary>
    public ErrorOr<Success> Update(string? displayName, string? description, int? priority = null)
    {
        // Guard: Prevent modifying default roles in restricted ways
        if (IsDefault) return RoleErrors.CannotModifyDefaultRole;
        
        // Guard: Prevent modifying critical system roles
        if (IsSystemRole) return RoleErrors.SystemRoleProtected;

        DisplayName = displayName?.Trim() ?? DisplayName;
        Description = description?.Trim() ?? Description;
        if (priority.HasValue) 
            Priority = Math.Clamp(priority.Value, RoleConstraints.MinPriority, RoleConstraints.MaxPriority);

        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new RoleEvents.RoleUpdated(this));
        return Result.Success;
    }

    /// <summary>
    /// Prepares the role for deletion by checking constraints.
    /// </summary>
    public ErrorOr<Deleted> Delete()
    {
        // Guard: Protect system integrity
        if (IsSystemRole) return RoleErrors.SystemRoleProtected;
        if (IsDefault) return RoleErrors.DefaultRoleProtected;
        
        // Guard: Ensure no users are orphaned
        if (UserRoles.Any()) return RoleErrors.RoleInUse(Name ?? Id);

        RaiseDomainEvent(new RoleEvents.RoleDeleted(Name ?? Id));
        return Result.Deleted;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    #endregion
}