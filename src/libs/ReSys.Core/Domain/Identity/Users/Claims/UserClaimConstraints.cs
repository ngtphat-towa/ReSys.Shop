namespace ReSys.Core.Domain.Identity.Users.Claims;

public static class UserClaimConstraints
{
    public const int MaxClaimsPerUser = 100;
    public const int ClaimTypeMaxLength = 256;
    public const int ClaimValueMaxLength = 1024;
    public const int AssignedByMaxLength = 256;
}