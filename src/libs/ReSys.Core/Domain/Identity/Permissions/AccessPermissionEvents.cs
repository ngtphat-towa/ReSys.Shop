using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Permissions;

public static class AccessPermissionEvents
{
    public record PermissionCreated(AccessPermission Permission) : IDomainEvent;
    public record PermissionUpdated(AccessPermission Permission) : IDomainEvent;
    public record PermissionDeleted(string PermissionName) : IDomainEvent;
}
