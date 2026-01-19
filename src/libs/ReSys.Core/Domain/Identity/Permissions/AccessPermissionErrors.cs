using ErrorOr;

namespace ReSys.Core.Domain.Identity.Permissions;

public static class AccessPermissionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Permission.NotFound",
        description: $"Permission with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Permission.NameRequired",
        description: "Permission name is required.");

    public static Error AreaRequired => Error.Validation(
        code: "Permission.AreaRequired",
        description: "Permission area is required.");

    public static Error ResourceRequired => Error.Validation(
        code: "Permission.ResourceRequired",
        description: "Permission resource is required.");

    public static Error ActionRequired => Error.Validation(
        code: "Permission.ActionRequired",
        description: "Permission action is required.");

    public static Error NameTooLong => Error.Validation(
        code: "Permission.NameTooLong",
        description: $"Permission name cannot exceed {AccessPermissionConstraints.MaxNameLength} characters.");

    public static Error InvalidFormat => Error.Validation(
        code: "Permission.InvalidFormat",
        description: "Permission name must follow the 'area.resource.action' format.");

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "Permission.DuplicateName",
        description: $"A permission with name '{name}' already exists.");
}