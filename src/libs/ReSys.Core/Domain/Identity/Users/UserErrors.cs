using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users;

public static class UserErrors
{
    public static Error NotFound => Error.NotFound(
        code: "User.NotFound",
        description: "The requested user does not exist.");

    public static Error EmailRequired => Error.Validation(
        code: "User.EmailRequired",
        description: "Email address is required.");

    public static Error DuplicateEmail => Error.Conflict(
        code: "User.DuplicateEmail",
        description: "Email address is already in use.");

    public static Error DuplicateUserName => Error.Conflict(
        code: "User.DuplicateUserName",
        description: "Username is already taken.");

    public static Error InvalidCredentials => Error.Validation(
        code: "Auth.InvalidCredentials",
        description: "The email or password provided is incorrect.");

    public static Error AccountLocked => Error.Validation(
        code: "Auth.AccountLocked",
        description: "This account has been locked due to too many failed attempts.");

    public static Error EmailNotConfirmed => Error.Validation(
        code: "Auth.EmailNotConfirmed",
        description: "You must confirm your email address before signing in.");
}
