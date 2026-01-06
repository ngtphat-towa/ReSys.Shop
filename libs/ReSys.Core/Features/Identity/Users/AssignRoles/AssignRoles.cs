using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Users.AssignRoles;

public static class AssignRoles
{
    public record Command(string Id, AssignRolesRequest Request) : IRequest<ErrorOr<Success>>;

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.Id);
            if (user == null) return IdentityErrors.UserNotFound;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var request = command.Request;
            
            // Add new roles
            var rolesToAdd = request.RoleNames.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return addResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            // Remove old roles
            var rolesToRemove = currentRoles.Except(request.RoleNames).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return removeResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return Result.Success;
        }
    }
}
