using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Users;

public static class UserEvents
{
    public record UserCreated(User User) : IDomainEvent;
    public record UserUpdated(User User) : IDomainEvent;
    public record ProfileUpdated(User User) : IDomainEvent;
    public record UserLoggedIn(User User) : IDomainEvent;
    public record RolesAssigned(User User, IEnumerable<string> Roles) : IDomainEvent;
}