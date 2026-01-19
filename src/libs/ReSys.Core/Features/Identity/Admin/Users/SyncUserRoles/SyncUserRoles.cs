using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Identity.Admin.Users.SyncUserRoles;

public static class SyncUserRoles
{
    public record Request(List<string> Roles);
    public record Command(string UserId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Roles).NotNull();
        }
    }

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null) return UserErrors.NotFound(command.UserId);

            var currentRoles = await userManager.GetRolesAsync(user);
            
            // 1. Remove Roles not in new list
            var rolesToRemove = currentRoles.Except(command.Request.Roles).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return removeResult.Errors.ToApplicationResult(prefix: "User.Sync");
            }

            // 2. Add Roles not in current list
            var rolesToAdd = command.Request.Roles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return addResult.Errors.ToApplicationResult(prefix: "User.Sync");
            }

            return Result.Success;
        }
    }
}