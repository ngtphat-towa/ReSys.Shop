using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Roles;

public static class RoleEvents
{
    public record RoleCreated(Role Role) : IDomainEvent;
    public record RoleUpdated(Role Role) : IDomainEvent;
    public record RoleDeleted(string RoleName) : IDomainEvent;
}