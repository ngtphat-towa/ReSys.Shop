using ErrorOr;

namespace ReSys.Core.Domain.Identity.Roles;

public static class RoleErrors
{
    public static Error NotFound => Error.NotFound(
        code: "Role.NotFound",
        description: "The specified role was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Role.NameRequired",
        description: "Role name cannot be empty.");

    public static Error DuplicateName => Error.Conflict(
        code: "Role.DuplicateName",
        description: "A role with this name already exists.");

    public static Error SystemRoleProtected => Error.Conflict(
        code: "Role.SystemRoleProtected",
        description: "System roles cannot be modified or deleted.");

    public static Error DefaultRoleProtected => Error.Conflict(
        code: "Role.DefaultRoleProtected",
        description: "The default role cannot be deleted.");
}
