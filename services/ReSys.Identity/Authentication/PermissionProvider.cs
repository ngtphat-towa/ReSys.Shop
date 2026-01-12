using Microsoft.EntityFrameworkCore;


using ReSys.Core.Common.Interfaces;
using ReSys.Identity.Persistence;

namespace ReSys.Identity.Authentication;

public class PermissionProvider(AppIdentityDbContext dbContext) : IPermissionProvider
{
    public async Task<IEnumerable<string>> GetPermissionsForUserAsync(string userId)
    {
        // 1. Get Direct User Permissions
        var userPermissions = await dbContext.UserClaims
            .Where(uc => uc.UserId == userId && uc.ClaimType == "permission")
            .Select(uc => uc.ClaimValue!)
            .ToListAsync();

        // 2. Get Role Permissions
        var rolePermissions = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(dbContext.RoleClaims, 
                ur => ur.RoleId, 
                rc => rc.RoleId, 
                (ur, rc) => rc)
            .Where(rc => rc.ClaimType == "permission")
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();

        return userPermissions.Union(rolePermissions).Distinct();
    }
}
