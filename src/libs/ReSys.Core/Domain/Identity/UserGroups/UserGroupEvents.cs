using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.UserGroups;

public static class UserGroupEvents
{
    public record GroupCreated(UserGroup Group) : IDomainEvent;
    public record GroupUpdated(UserGroup Group) : IDomainEvent;
    public record GroupDeleted(Guid GroupId, string Name) : IDomainEvent;
    public record UserJoinedGroup(Guid GroupId, string UserId, bool IsPrimary) : IDomainEvent;
    public record UserLeftGroup(Guid GroupId, string UserId) : IDomainEvent;
}
