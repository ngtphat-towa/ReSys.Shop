using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Management.Users;

public static class UpdateRoles
{
    public record Request(List<string> Roles);
    public record Command(string UserId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Roles).NotNull();
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("Users.NotFound", "User not found");

            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToAdd = request.Data.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(request.Data.Roles).ToList();

            await userManager.AddToRolesAsync(user, rolesToAdd);
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return Result.Success;
        }
    }
}
