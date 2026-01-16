using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.UserGroups;

public static class UserGroupEvents
{
    public record UserGroupCreated(UserGroup Group) : IDomainEvent;
    public record UserGroupUpdated(UserGroup Group) : IDomainEvent;
    public record UserGroupDeleted(Guid GroupId, string Code) : IDomainEvent;
}
