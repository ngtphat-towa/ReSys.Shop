using ErrorOr;

namespace ReSys.Core.Features.Identity.Common;

public static class IdentityErrors
{
    public static Error RoleExists => Error.Conflict(
        code: "Role.Exists",
        description: "Role already exists.");

    public static Error RoleNotFound => Error.NotFound(
        code: "Role.NotFound",
        description: "Role not found.");

    public static Error UserNotFound => Error.NotFound(
        code: "User.NotFound",
        description: "User not found.");
        
    public static Error EmailAlreadyInUse => Error.Conflict(
        code: "User.EmailAlreadyInUse",
        description: "Email is already in use.");
}
