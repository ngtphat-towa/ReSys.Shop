using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Roles.DeleteRole;

public static class DeleteRole
{
    public record Command(string Id) : IRequest<ErrorOr<Success>>;

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

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return Result.Success;
        }
    }
}
