using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;
using ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Domain.Identity.Users.Roles;
using ReSys.Core.Domain.Identity.Users.Claims;
using ReSys.Core.Domain.Identity.Users.Logins;
using ReSys.Core.Domain.Identity.Users.Tokens;

using System.Text.RegularExpressions;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users;

/// <summary>
/// Represents a user account within the system, serving as an aggregate root
/// for managing identity, authentication, profile information, and associated entities.
/// </summary>
public class User : IdentityUser, IAuditable, IAggregate, IHasMetadata, IHasVersion
{
    #region Properties
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? ProfileImagePath { get; set; }

    // Identity & Security State
    public DateTimeOffset? LastSignInAt { get; set; }
    public string? LastIpAddress { get; set; }
    public DateTimeOffset? CurrentSignInAt { get; set; }
    public string? CurrentSignInIp { get; set; }
    public int SignInCount { get; set; }
    public bool IsActive { get; set; } = true;

    // Profiles
    public virtual CustomerProfile? CustomerProfile { get; set; }
    public virtual StaffProfile? StaffProfile { get; set; }

    // Relationships
    public virtual ICollection<UserGroupMembership> GroupMemberships { get; set; } = new List<UserGroupMembership>();
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();
    public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
    public virtual ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();

    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // IHasVersion
    public long Version { get; set; }

    // IAggregate
    private readonly List<object> _domainEvents = [];
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void RaiseDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);

    public string FullName => $"{FirstName} {LastName}".Trim();
    #endregion

    protected User() { }

    #region Factory Methods
    /// <summary>
    /// Creates a new user aggregate root.
    /// </summary>
    public static ErrorOr<User> Create(
        string email,
        string? userName = null,
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        bool emailConfirmed = false)
    {
        // Guard: Email is mandatory
        if (string.IsNullOrWhiteSpace(email)) return UserErrors.EmailRequired;

        // Guard: Prevent extreme lengths
        if (email.Length > UserConstraints.EmailMaxLength) return UserErrors.InvalidCredentials;

        // Guard: Validate format against business rules
        if (!Regex.IsMatch(email, UserConstraints.EmailPattern)) return UserErrors.InvalidCredentials;

        string effectiveUserName = string.IsNullOrWhiteSpace(userName) ? email.Split('@').First() : userName.Trim();

        // Guard: Validate username format
        if (!Regex.IsMatch(effectiveUserName, UserConstraints.UserNamePattern)) return UserErrors.InvalidCredentials;

        // Guard: Validate phone length if provided
        if (phoneNumber != null && phoneNumber.Length > UserConstraints.PhoneMaxLength) return UserErrors.InvalidCredentials;

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.ToLowerInvariant(),
            UserName = effectiveUserName,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = effectiveUserName.ToUpperInvariant(),
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            EmailConfirmed = emailConfirmed,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        };

        // Business Rule: Every new user must trigger initial identity events
        user.RaiseDomainEvent(new UserEvents.UserCreated(user));
        return user;
    }
    #endregion

    #region Business Logic
    /// <summary>
    /// Updates personal profile information.
    /// </summary>
    public ErrorOr<Success> UpdateProfile(string? firstName, string? lastName, DateTimeOffset? dateOfBirth, string? profileImagePath = null)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        DateOfBirth = dateOfBirth;
        if (profileImagePath != null) ProfileImagePath = profileImagePath;

        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.ProfileUpdated(this));
        return Result.Success;
    }

    /// <summary>
    /// Updates the active status of the user account.
    /// </summary>
    public ErrorOr<Success> UpdateStatus(bool isActive)
    {
        if (IsActive == isActive) return Result.Success;

        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.StatusChanged(this, isActive));
        return Result.Success;
    }

    /// <summary>
    /// Records a successful sign-in attempt and updates security metrics.
    /// </summary>
    public void RecordSignIn(string? ipAddress = null)
    {
        LastSignInAt = CurrentSignInAt;
        LastIpAddress = CurrentSignInIp;
        CurrentSignInAt = DateTimeOffset.UtcNow;
        CurrentSignInIp = ipAddress ?? "Unknown";
        SignInCount++;

        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.UserLoggedIn(this));
    }

    /// <summary>
    /// Manually locks the account for security reasons.
    /// </summary>
    public void LockAccount(DateTimeOffset? lockoutEnd = null)
    {
        LockoutEnabled = true;
        LockoutEnd = lockoutEnd ?? DateTimeOffset.UtcNow.AddYears(100);
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.AccountLocked(this, LockoutEnd));
    }

    /// <summary>
    /// Releases an existing account lockout.
    /// </summary>
    public void UnlockAccount()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.AccountUnlocked(this));
    }

    /// <summary>
    /// Triggers a password reset request event.
    /// </summary>
    public void RequestPasswordReset()
    {
        RaiseDomainEvent(new UserEvents.PasswordResetRequested(this));
        // We set UpdatedAt to ensure the entity is tracked as modified so interceptors fire
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Triggers an email change request event.
    /// </summary>
    public void RequestEmailChange(string newEmail)
    {
        RaiseDomainEvent(new UserEvents.EmailChangeRequested(this, newEmail));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Triggers a phone change request event.
    /// </summary>
    public void RequestPhoneChange(string newPhone)
    {
        RaiseDomainEvent(new UserEvents.PhoneChangeRequested(this, newPhone));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Triggers an email confirmation request event.
    /// </summary>
    public void RequestEmailConfirmation()
    {
        RaiseDomainEvent(new UserEvents.EmailConfirmationRequested(this));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Triggers a phone confirmation request event.
    /// </summary>
    public void RequestPhoneConfirmation()
    {
        RaiseDomainEvent(new UserEvents.PhoneConfirmationRequested(this));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public ErrorOr<Deleted> Delete()
    {
        // Guard: Prevent deleting user with active sessions
        if (RefreshTokens.Any(t => t.IsActive)) return UserErrors.HasActiveTokens;

        // Guard: Prevent deleting user with assigned roles (must be unassigned first)
        if (UserRoles != null && UserRoles.Any()) return UserErrors.HasActiveRoles;

        RaiseDomainEvent(new UserEvents.UserDeleted(Id, Email ?? string.Empty));
        return Result.Deleted;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Checks if the user is in a specific role.
    /// </summary>
    public bool HasRole(string roleId) => UserRoles.Any(ur => ur.RoleId == roleId);

    /// <summary>
    /// Assigns a role to the user.
    /// </summary>
    public ErrorOr<Success> AssignRole(string roleId, string roleName)
    {
        if (HasRole(roleId)) return UserErrors.RoleAlreadyAssigned(roleName);

        UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.RoleAssigned(this, roleName));
        return Result.Success;
    }

    /// <summary>
    /// Unassigns a role from the user.
    /// </summary>
    public ErrorOr<Success> UnassignRole(string roleId, string roleName)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null) return UserErrors.RoleNotAssigned(roleName);

        UserRoles.Remove(userRole);
        UpdatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserEvents.RoleUnassigned(this, roleName));
        return Result.Success;
    }

    /// <summary>
    /// Adds a new address to the user's address collection.
    /// </summary>
    public void AddAddress(UserAddress address)
    {
        UserAddresses.Add(address);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets a specific address as default and unmarks all others.
    /// </summary>
    public void SetDefaultAddress(Guid addressId)
    {
        foreach (var address in UserAddresses)
        {
            if (address.Id == addressId)
                address.MarkAsDefault();
            else
                address.UnmarkAsDefault();
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Business Logic: Profile Management
    /// <summary>
    /// Ensures the user has a customer profile; creates one if absent.
    /// </summary>
    public void EnsureCustomerProfile()
    {
        CustomerProfile ??= CustomerProfile.Create(Id);
    }

    public ErrorOr<Success> EnsureStaffProfile(string? employeeId = null)
    {
        if (StaffProfile != null) return Result.Success;

        var profileResult = StaffProfile.Create(Id, employeeId);
        if (profileResult.IsError) return profileResult.Errors;

        StaffProfile = profileResult.Value;
        return Result.Success;
    }

    // Business Logic: Group Management
    /// <summary>
    /// Adds the user to a group. If isPrimary is true, other memberships are demoted.
    /// </summary>
    public ErrorOr<Success> JoinGroup(Guid groupId, bool isPrimary = false)
    {
        if (GroupMemberships.Any(m => m.UserGroupId == groupId))
            return UserGroupErrors.UserAlreadyInGroup;

        if (isPrimary)
        {
            foreach (var m in GroupMemberships) m.SetPrimary(false);
        }

        var membershipResult = UserGroupMembership.Create(Id, groupId, isPrimary: isPrimary);
        if (membershipResult.IsError) return membershipResult.Errors;

        GroupMemberships.Add(membershipResult.Value);

        RaiseDomainEvent(new UserGroupEvents.UserJoinedGroup(groupId, Id, isPrimary));
        return Result.Success;
    }

    /// <summary>
    /// Removes the user from a group.
    /// </summary>
    public ErrorOr<Success> LeaveGroup(Guid groupId)
    {
        var membership = GroupMemberships.FirstOrDefault(m => m.UserGroupId == groupId);
        if (membership == null) return UserGroupErrors.UserNotInGroup;

        GroupMemberships.Remove(membership);

        RaiseDomainEvent(new UserGroupEvents.UserLeftGroup(groupId, Id));
        return Result.Success;
    }
    #endregion
}
