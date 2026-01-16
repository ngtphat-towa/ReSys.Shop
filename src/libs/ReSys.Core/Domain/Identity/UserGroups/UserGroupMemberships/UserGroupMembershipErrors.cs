using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;

public static class UserGroupMembershipErrors
{
    public static Error NotFound => Error.NotFound(
        code: "UserGroupMembership.NotFound",
        description: "User is not a member of this group.");
    
    public static Error AlreadyMember => Error.Conflict(
        code: "UserGroupMembership.AlreadyMember",
        description: "User is already a member of this group.");
}
