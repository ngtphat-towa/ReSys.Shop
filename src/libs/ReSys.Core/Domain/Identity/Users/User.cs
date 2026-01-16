using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;
using ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;
using ErrorOr;
using System.Text.RegularExpressions;

namespace ReSys.Core.Domain.Identity.Users;

public class User : IdentityUser, IAuditable, IAggregate, IHasMetadata
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? ProfileImagePath { get; set; }
    
    // Identity & Security
    public DateTimeOffset? LastSignInAt { get; set; }
    public string? LastIpAddress { get; set; }
    public int SignInCount { get; set; }
    public bool IsActive { get; set; } = true;

    // Profiles (Composition)
    public virtual CustomerProfile? CustomerProfile { get; set; }
    public virtual StaffProfile? StaffProfile { get; set; }

    // Relationships (Explicit Join Entity)
    public virtual ICollection<UserGroupMembership> GroupMemberships { get; set; } = new List<UserGroupMembership>();

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

    public string FullName => $"{FirstName} {LastName}".Trim();

    private User() { }

    public static ErrorOr<User> Create(string email, string? firstName = null, string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return UserErrors.EmailRequired;
        if (!Regex.IsMatch(email, UserConstraints.EmailPattern)) return UserErrors.InvalidCredentials;

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.ToLowerInvariant(),
            UserName = email.ToLowerInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.RaiseDomainEvent(new UserEvents.UserCreated(user));
        return user;
    }

    // Business Logic: Profile Management
    public void EnsureCustomerProfile()
    {
        CustomerProfile ??= CustomerProfile.Create(Id);
    }

    public void EnsureStaffProfile(string? employeeId = null)
    {
        StaffProfile ??= StaffProfile.Create(Id, employeeId);
    }

    // Business Logic: Group Management
    public void AddToGroup(Guid groupId, string? assignedBy = null, bool isPrimary = false)
    {
        if (!GroupMemberships.Any(m => m.UserGroupId == groupId))
        {
            GroupMemberships.Add(UserGroupMembership.Create(Id, groupId, assignedBy, isPrimary));
        }
    }

    public void RemoveFromGroup(Guid groupId)
    {
        var membership = GroupMemberships.FirstOrDefault(m => m.UserGroupId == groupId);
        if (membership != null)
        {
            GroupMemberships.Remove(membership);
        }
    }

    // Business Logic: Activity Tracking
    public void RecordSignIn(string? ipAddress = null)
    {
        LastSignInAt = DateTimeOffset.UtcNow;
        LastIpAddress = ipAddress;
        SignInCount++;
        RaiseDomainEvent(new UserEvents.UserLoggedIn(this));
    }
}