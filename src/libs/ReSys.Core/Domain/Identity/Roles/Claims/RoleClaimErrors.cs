using ErrorOr;

namespace ReSys.Core.Domain.Identity.Roles.Claims;

public static class RoleClaimErrors
{
    public static Error RoleIdRequired => Error.Validation(
        code: "RoleClaim.RoleIdRequired",
        description: "Role ID is required.");
    
    public static Error ClaimTypeRequired => Error.Validation(
        code: "RoleClaim.ClaimTypeRequired",
        description: "Claim type is required.");

    public static Error ClaimTypeTooLong(int max) => Error.Validation(
        code: "RoleClaim.ClaimTypeTooLong",
        description: $"Claim type cannot exceed {max} characters.");

    public static Error ClaimValueTooLong(int max) => Error.Validation(
        code: "RoleClaim.ClaimValueTooLong",
        description: $"Claim value cannot exceed {max} characters.");

    public static Error AssignedByTooLong(int max) => Error.Validation(
        code: "RoleClaim.AssignedByTooLong",
        description: $"Assigned by cannot exceed {max} characters.");
}