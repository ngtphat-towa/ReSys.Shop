using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using ReSys.Core.Common.Interfaces;

namespace ReSys.Identity.Authentication;

public class PermissionClaimsTransformation(
    IPermissionProvider permissionProvider,
    IMemoryCache cache) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if already transformed to avoid re-running logic in the same request
        if (principal.HasClaim(c => c.Type == "permissions_loaded"))
        {
            return principal;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        // Cache permissions for 5 minutes to reduce DB load
        var permissions = await cache.GetOrCreateAsync($"perms_{userId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await permissionProvider.GetPermissionsForUserAsync(userId);
        });

        if (permissions == null) return principal;

        var clone = principal.Clone();
        var newIdentity = clone.Identity as ClaimsIdentity;

        if (newIdentity == null) return principal;

        foreach (var permission in permissions)
        {
            newIdentity.AddClaim(new Claim("permission", permission));
        }

        // Mark as loaded
        newIdentity.AddClaim(new Claim("permissions_loaded", "true"));

        return clone;
    }
}
