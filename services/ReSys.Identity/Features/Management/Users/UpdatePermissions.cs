using System.Security.Claims;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Management.Users;

public static class UpdatePermissions
{
    public record Request(List<string> Permissions);
    public record Command(string UserId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Permissions).NotNull();
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("Users.NotFound", "User not found");

            var currentClaims = await userManager.GetClaimsAsync(user);
            var currentPermissions = currentClaims.Where(c => c.Type == "permission").ToList();

            var permissionsToRemove = currentPermissions
                .Where(c => !request.Data.Permissions.Contains(c.Value))
                .ToList();

            if (permissionsToRemove.Any())
            {
                await userManager.RemoveClaimsAsync(user, permissionsToRemove);
            }

            var existingValues = currentPermissions.Select(c => c.Value).ToHashSet();
            var permissionsToAdd = request.Data.Permissions
                .Where(p => !existingValues.Contains(p))
                .Distinct()
                .Select(p => new Claim("permission", p))
                .ToList();

            if (permissionsToAdd.Any())
            {
                await userManager.AddClaimsAsync(user, permissionsToAdd);
            }

            return Result.Success;
        }
    }
}
