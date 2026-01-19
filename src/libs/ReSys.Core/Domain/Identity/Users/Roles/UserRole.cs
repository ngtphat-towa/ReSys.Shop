using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Identity.Roles;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Roles;

public sealed class UserRole : IdentityUserRole<string>, IHasAssignable
{
    public static class Constraints
    {
        public const int MaxRolePerUser = 10;
        public const int MaxUsersPerRole = 1000;
    }

    public DateTimeOffset? AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
    public string? AssignedTo { get; set; }

    public Role Role { get; set; } = null!;
    public User User { get; set; } = null!;

    public static ErrorOr<UserRole> Create(string userId, string roleId, string? assignedBy = null)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedTo = userId,
            AssignedBy = assignedBy // Interceptor will overwrite if empty, but good to have as option
        };
    }
}
