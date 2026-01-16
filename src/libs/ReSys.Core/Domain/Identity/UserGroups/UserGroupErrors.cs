using ErrorOr;

namespace ReSys.Core.Domain.Identity.UserGroups;

public static class UserGroupErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "UserGroup.NotFound",
        description: $"User group with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "UserGroup.NameRequired",
        description: "Group name is required.");

    public static Error CodeRequired => Error.Validation(
        code: "UserGroup.CodeRequired",
        description: "Group code is required.");

    public static Error DuplicateCode(string code) => Error.Conflict(
        code: "UserGroup.DuplicateCode",
        description: $"Group with code '{code}' already exists.");

    public static Error HasMembers => Error.Conflict(
        code: "UserGroup.HasMembers",
        description: "Cannot delete a group that still has members.");
}