using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Common;

public static class AccountResultExtensions
{
    /// <summary>
    /// Translates IdentityError collections into Domain ErrorOr lists.
    /// </summary>
    public static List<Error> ToApplicationResult(
        this IEnumerable<IdentityError> errors,
        string prefix = "Auth",
        string fallbackCode = "UnknownError")
    {
        return errors.Select(error => Error.Validation(
            code: !string.IsNullOrWhiteSpace(error.Code) ? $"{prefix}.{error.Code}" : $"{prefix}.{fallbackCode}",
            description: error.Description)).ToList();
    }

    /// <summary>
    /// Maps a SignInResult to a specific Domain Error.
    /// Addresses Gap #3: Granular Login Feedback.
    /// </summary>
    public static Error MapToError(this SignInResult result)
    {
        if (result.IsLockedOut) return UserErrors.AccountLocked;
        if (result.IsNotAllowed) return UserErrors.EmailNotConfirmed;
        if (result.RequiresTwoFactor) return Error.Validation("Auth.TwoFactorRequired", "Two-factor authentication is required.");
        
        return UserErrors.InvalidCredentials;
    }
}