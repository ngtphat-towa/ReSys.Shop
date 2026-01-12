using System.Security.Claims;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ReSys.Identity.Features.Management.Roles;

public static class UpdatePermissions
{
    public record Request(List<string> Permissions);

    public record Command(string RoleId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Permissions).NotNull();
        }
    }

    public class Handler(RoleManager<IdentityRole> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return Error.NotFound("Roles.NotFound", "Role not found");

            var currentClaims = await roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims.Where(c => c.Type == "permission").ToList();

            // Determine which to remove
            var permissionsToRemove = currentPermissions
                .Where(c => !request.Data.Permissions.Contains(c.Value))
                .ToList();

            foreach (var claim in permissionsToRemove)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            // Determine which to add
            var existingValues = currentPermissions.Select(c => c.Value).ToHashSet();
            var permissionsToAdd = request.Data.Permissions
                .Where(p => !existingValues.Contains(p))
                .Distinct()
                .ToList();

            foreach (var permission in permissionsToAdd)
            {
                await roleManager.AddClaimAsync(role, new Claim("permission", permission));
            }

            return Result.Success;
        }
    }
}
