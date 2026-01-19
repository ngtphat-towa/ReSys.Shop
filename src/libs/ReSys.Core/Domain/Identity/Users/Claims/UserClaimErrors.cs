using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Claims;

public static class UserClaimErrors
{
    public static Error UserIdRequired => Error.Validation(
        code: "UserClaim.UserIdRequired",
        description: "User ID is required.");
    
    public static Error ClaimTypeRequired => Error.Validation(
        code: "UserClaim.ClaimTypeRequired",
        description: "Claim type is required.");

    public static Error ClaimTypeTooLong(int max) => Error.Validation(
        code: "UserClaim.ClaimTypeTooLong",
        description: $"Claim type cannot exceed {max} characters.");

    public static Error ClaimValueTooLong(int max) => Error.Validation(
        code: "UserClaim.ClaimValueTooLong",
        description: $"Claim value cannot exceed {max} characters.");

    public static Error AssignedByTooLong(int max) => Error.Validation(
        code: "UserClaim.AssignedByTooLong",
        description: $"Assigned by cannot exceed {max} characters.");
}