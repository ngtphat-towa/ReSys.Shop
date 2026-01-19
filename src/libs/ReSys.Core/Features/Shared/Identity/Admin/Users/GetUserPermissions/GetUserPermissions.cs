using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Common.Security.Authorization.Claims;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.GetUserPermissions;

public static class GetUserPermissions
{
    public record Response
    {
        public List<string> RolePermissions { get; init; } = [];
        public List<string> DirectPermissions { get; init; } = [];
        public List<string> EffectivePermissions { get; init; } = [];
    }

    public record Query(string UserId) : IRequest<ErrorOr<Response>>;

    public class Handler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return UserErrors.NotFound(request.UserId);

            // 1. Get Roles and their permissions
            var roles = await userManager.GetRolesAsync(user);
            var rolePermissions = new HashSet<string>();
            foreach (var roleName in roles)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await roleManager.GetClaimsAsync(role);
                    foreach (var c in claims.Where(c => c.Type == CustomClaim.Permission))
                        rolePermissions.Add(c.Value);
                }
            }

            // 2. Get direct permissions
            var userClaims = await userManager.GetClaimsAsync(user);
            var directPermissions = userClaims
                .Where(c => c.Type == CustomClaim.Permission)
                .Select(c => c.Value)
                .ToList();

            // 3. Calculate Effective (Union)
            var effective = new HashSet<string>(rolePermissions);
            foreach (var p in directPermissions) effective.Add(p);

            return new Response
            {
                RolePermissions = rolePermissions.OrderBy(p => p).ToList(),
                DirectPermissions = directPermissions.OrderBy(p => p).ToList(),
                EffectivePermissions = effective.OrderBy(p => p).ToList()
            };
        }
    }
}
