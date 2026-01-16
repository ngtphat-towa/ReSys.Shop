using ErrorOr;

namespace ReSys.Core.Domain.Identity.Permissions;

public static class AccessPermissionErrors
{
    public static Error NotFound(string name) => Error.NotFound(
        code: "AccessPermission.NotFound",
        description: $"Permission '{name}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "AccessPermission.NameRequired",
        description: "Permission name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "AccessPermission.NameTooLong",
        description: $"Permission name cannot exceed {AccessPermissionConstraints.NameMaxLength} characters.");

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "AccessPermission.DuplicateName",
        description: $"Permission with name '{name}' already exists.");
}