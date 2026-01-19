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

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "Role.DuplicateName",
        description: $"A role with name '{name}' already exists.");

    public static Error SystemRoleProtected => Error.Conflict(
        code: "Role.SystemRoleProtected",
        description: "System roles cannot be modified or deleted.");

    public static Error CannotDeleteSystemRole => Error.Conflict(
        code: "Role.CannotDeleteSystemRole",
        description: "The specified role is a system role and cannot be deleted.");

    public static Error DefaultRoleProtected => Error.Conflict(
        code: "Role.DefaultRoleProtected",
        description: "The default role cannot be deleted.");

    public static Error CannotModifyDefaultRole => Error.Validation(
        code: "Role.CannotModifyDefaultRole",
        description: "Default roles are protected from certain modifications to maintain system integrity.");

    public static Error RoleInUse(string roleName) => Error.Validation(
        code: "Role.RoleInUse",
        description: $"Cannot delete role '{roleName}' because it is assigned to one or more users.");
}
