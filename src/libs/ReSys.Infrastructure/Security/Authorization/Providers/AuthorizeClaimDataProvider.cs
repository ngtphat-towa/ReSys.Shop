using System.Security.Claims;
using System.Text.Json;
using AsyncKeyedLock;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Infrastructure.Security.Authorization.Options;

namespace ReSys.Infrastructure.Security.Authorization.Providers;

public sealed class AuthorizeClaimDataProvider(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IDistributedCache cache,
    IOptions<AuthUserCacheOption> cacheOptions) : IAuthorizeClaimDataProvider
{
    private static readonly AsyncKeyedLocker<string> UserLocks = new();
    private readonly AuthUserCacheOption _options = cacheOptions.Value;

    public async Task<AuthorizeClaimData?> GetUserAuthorizationAsync(string userId)
    {
        string cacheKey = $"UserAuth_{userId}";
        string? cachedData = await cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return JsonSerializer.Deserialize<AuthorizeClaimData>(cachedData);

        using (await UserLocks.LockAsync(userId))
        {
            // Re-check after lock
            cachedData = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
                return JsonSerializer.Deserialize<AuthorizeClaimData>(cachedData);

            return await FetchAndCacheAuthData(userId, cacheKey);
        }
    }

    private async Task<AuthorizeClaimData?> FetchAndCacheAuthData(string userId, string cacheKey)
    {
        var user = await userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var roleEntities = await roleManager.Roles.Where(r => roles.Contains(r.Name!)).ToListAsync();
        
        var allClaims = new List<Claim>();
        foreach (var role in roleEntities)
        {
            allClaims.AddRange(await roleManager.GetClaimsAsync(role));
        }
        allClaims.AddRange(await userManager.GetClaimsAsync(user));

        var authData = new AuthorizeClaimData(
            UserId: userId,
            UserName: user.UserName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Permissions: GetDistinctValues(allClaims, CustomClaim.Permission),
            Roles: roles.ToList().AsReadOnly(),
            Policies: GetDistinctValues(allClaims, CustomClaim.Policy)
        );

        var serialized = JsonSerializer.Serialize(authData);
        await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.UserAuthCacheExpiryInMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_options.UserAuthCacheSlidingInMinutes)
        });

        return authData;
    }

    public async Task InvalidateUserAuthorizationAsync(string userId)
    {
        await cache.RemoveAsync($"UserAuth_{userId}");
    }

    private static IReadOnlyList<string> GetDistinctValues(IEnumerable<Claim> claims, string type) =>
        claims.Where(c => c.Type == type).Select(c => c.Value).Distinct().ToList().AsReadOnly();
}
