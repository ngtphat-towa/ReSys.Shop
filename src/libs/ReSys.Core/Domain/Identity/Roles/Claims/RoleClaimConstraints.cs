namespace ReSys.Core.Domain.Identity.Roles.Claims;

public static class RoleClaimConstraints
{
    public const int MaxClaimsPerRole = 100;
    public const int ClaimTypeMaxLength = 256;
    public const int ClaimValueMaxLength = 1024;
    public const int AssignedByMaxLength = 256;
}