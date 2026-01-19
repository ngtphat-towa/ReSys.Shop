using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users;

public static class UserErrors
{
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

    public static Error NotFound(string credential) => Error.NotFound(
        code: "User.NotFound",
        description: $"User with credential '{credential}' was not found.");

    public static Error UserNameAlreadyExists(string userName) => Error.Conflict(
        code: "User.UserNameAlreadyExists",
        description: $"Username '{userName}' already exists.");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        code: "User.EmailAlreadyExists",
        description: $"Email '{email}' already exists.");

    public static Error PhoneNumberAlreadyExists(string phoneNumber) => Error.Conflict(
        code: "User.PhoneNumberAlreadyExists",
        description: $"Phone number '{phoneNumber}' already exists.");

    public static Error InvalidToken => Error.Validation(
        code: "User.InvalidToken",
        description: "The provided security token is invalid or expired.");

    public static Error HasActiveTokens => Error.Validation(
        code: "User.HasActiveTokens",
        description: "Cannot delete user with active refresh tokens.");

    public static Error HasActiveRoles => Error.Validation(
        code: "User.HasActiveRoles",
        description: "Cannot delete user with assigned roles.");

    public static Error RoleAlreadyAssigned(string role) => Error.Conflict(
        code: "User.RoleAlreadyAssigned",
        description: $"User is already assigned to the '{role}' role.");

    public static Error RoleNotAssigned(string role) => Error.NotFound(
        code: "User.RoleNotAssigned",
        description: $"User is not assigned to the '{role}' role.");

    public static Error Unauthorized => Error.Unauthorized(
        code: "User.Unauthorized",
        description: "You do not have permission to access this resource.");
}
