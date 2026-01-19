using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups;

/// <summary>
/// Predefined business errors for User Group operations.
/// </summary>
public static class UserGroupErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "UserGroup.NotFound",
        description: $"User group with ID '{id}' was not found.");

    public static Error DuplicateName(string name) => Error.Conflict(
        code: "UserGroup.DuplicateName",
        description: $"A group with name '{name}' already exists.");

    public static Error DuplicateCode(string code) => Error.Conflict(
        code: "UserGroup.DuplicateCode",
        description: $"A group with code '{code}' already exists.");

    public static Error UserAlreadyInGroup => Error.Conflict(
        code: "UserGroup.UserAlreadyInGroup",
        description: "The user is already a member of this group.");

    public static Error UserNotInGroup => Error.NotFound(
        code: "UserGroup.UserNotInGroup",
        description: "The user is not a member of this group.");

    public static Error NameRequired => Error.Validation(
        code: "UserGroup.NameRequired",
        description: "Group name is required.");

    public static Error CodeRequired => Error.Validation(
        code: "UserGroup.CodeRequired",
        description: "Group code is required.");
}