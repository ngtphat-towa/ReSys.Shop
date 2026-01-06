using System.Security.Claims;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Roles.UpdateRolePermissions;

public static class UpdateRolePermissions
{
    public record Command(string Id, UpdateRolePermissionsRequest Request) : IRequest<ErrorOr<Success>>;

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public Handler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(command.Id);
            if (role == null) return IdentityErrors.RoleNotFound;

            // Get existing claims
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims.Where(c => c.Type == "permission").ToList();

            // Determine what to add and remove
            // We assume request.Permissions is the FULL new list
            
            // Remove ones not in new list
            foreach (var claim in existingPermissions)
            {
                if (!command.Request.Permissions.Contains(claim.Value))
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            // Add ones not in old list
            foreach (var permission in command.Request.Permissions)
            {
                if (!existingPermissions.Any(c => c.Value == permission))
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
                }
            }

            return Result.Success;
        }
    }
}
