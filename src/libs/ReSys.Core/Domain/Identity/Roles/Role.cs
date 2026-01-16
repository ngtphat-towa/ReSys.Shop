using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Roles;

public class Role : IdentityRole, IAuditable, IAggregate, IHasMetadata
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystemRole { get; set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // IAggregate
    private readonly List<object> _domainEvents = [];
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void RaiseDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);

    private Role() { }

    public static ErrorOr<Role> Create(string name, string? displayName = null, bool isDefault = false, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return RoleErrors.NameRequired;

        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            DisplayName = displayName?.Trim() ?? name.Trim(),
            IsDefault = isDefault,
            IsSystemRole = isSystemRole,
            CreatedAt = DateTimeOffset.UtcNow
        };

        role.RaiseDomainEvent(new RoleEvents.RoleCreated(role));
        return role;
    }

    public ErrorOr<Success> Update(string? displayName, string? description)
    {
        if (IsSystemRole) return RoleErrors.SystemRoleProtected;

        DisplayName = displayName?.Trim();
        Description = description?.Trim();

        RaiseDomainEvent(new RoleEvents.RoleUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsSystemRole) return RoleErrors.SystemRoleProtected;
        if (IsDefault) return RoleErrors.DefaultRoleProtected;

        RaiseDomainEvent(new RoleEvents.RoleDeleted(Name ?? Id));
        return Result.Deleted;
    }
}
