using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Roles.UnassignPermissionFromRole;

public static class UnassignPermissionFromRole
{
    public record Command(string RoleId, string PermissionName) : IRequest<ErrorOr<Success>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId);
            if (role == null) return RoleErrors.NotFound;

            var claims = await roleManager.GetClaimsAsync(role);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == CustomClaim.Permission && c.Value == command.PermissionName);

            if (claimToRemove == null) return Result.Success;

            var result = await roleManager.RemoveClaimAsync(role, claimToRemove);
            return result.Succeeded ? Result.Success : Error.Failure("Role.UnassignFailed");
        }
    }
}