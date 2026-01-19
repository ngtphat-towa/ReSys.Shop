using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using System.Security.Claims;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.SyncRolePermissions;

public static class SyncRolePermissions
{
    public record Request(List<string> Permissions);
    public record Command(string RoleId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId);
            if (role == null) return RoleErrors.NotFound;

            var currentClaims = await roleManager.GetClaimsAsync(role);
            var currentPerms = currentClaims
                .Where(c => c.Type == CustomClaim.Permission)
                .Select(c => c.Value)
                .ToList();

            // 1. Remove permissions not in new list
            var permsToRemove = currentPerms.Except(command.Request.Permissions).ToList();
            foreach (var p in permsToRemove)
            {
                var claim = currentClaims.First(c => c.Type == CustomClaim.Permission && c.Value == p);
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // 2. Add new permissions
            var permsToAdd = command.Request.Permissions.Except(currentPerms).ToList();
            foreach (var p in permsToAdd)
            {
                await roleManager.AddClaimAsync(role, new Claim(CustomClaim.Permission, p));
            }

            return Result.Success;
        }
    }
}
