using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Users;

public static class UserEvents
{
    public record UserCreated(User User) : IDomainEvent;
    public record UserUpdated(User User) : IDomainEvent;
    public record ProfileUpdated(User User) : IDomainEvent;
    public record UserLoggedIn(User User) : IDomainEvent;
    public record RolesAssigned(User User, IEnumerable<string> Roles) : IDomainEvent;
    public record AccountLocked(User User, DateTimeOffset? EndDate) : IDomainEvent;
    public record AccountUnlocked(User User) : IDomainEvent;
    public record StatusChanged(User User, bool IsActive) : IDomainEvent;
    public record PasswordResetRequested(User User) : IDomainEvent;
    public record EmailChangeRequested(User User, string NewEmail) : IDomainEvent;
    public record PhoneChangeRequested(User User, string NewPhone) : IDomainEvent;
    public record EmailConfirmationRequested(User User) : IDomainEvent;
    public record PhoneConfirmationRequested(User User) : IDomainEvent;
    public record RoleAssigned(User User, string RoleName) : IDomainEvent;
    public record RoleUnassigned(User User, string RoleName) : IDomainEvent;
    public record UserDeleted(string UserId, string Email) : IDomainEvent;
}
