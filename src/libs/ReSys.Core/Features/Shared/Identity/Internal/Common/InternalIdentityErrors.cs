using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Internal.Common;

public static class InternalIdentityErrors
{
    public static Error InvalidCredentials => Error.Validation(
        code: "Identity.InvalidCredentials",
        description: "The provided credentials are invalid.");

    public static Error Unauthorized => Error.Validation(
        code: "Identity.Unauthorized",
        description: "You are not authorized to perform this action.");

    public static Error NotFound(string userId) => Error.NotFound(
        code: "Identity.UserNotFound",
        description: $"User with ID '{userId}' was not found.");
}
