using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ErrorOr;
using System.Security.Claims;

namespace ReSys.Core.Features.Identity.Admin.Roles.AssignPermissionToRole;

public static class AssignPermissionToRole
{
    public record Request(string PermissionName);
    public record Command(string RoleId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var role = await roleManager.FindByIdAsync(command.RoleId);
            if (role == null) return RoleErrors.NotFound;

            var claims = await roleManager.GetClaimsAsync(role);
            if (claims.Any(c => c.Type == CustomClaim.Permission && c.Value == command.Request.PermissionName))
                return Result.Success;

            var result = await roleManager.AddClaimAsync(role, new Claim(CustomClaim.Permission, command.Request.PermissionName));
            return result.Succeeded ? Result.Success : Error.Failure("Role.AssignFailed");
        }
    }
}