using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.DeleteRole;

public static class DeleteRole
{
    public record Command(string Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var role = await roleManager.FindByIdAsync(command.Id);
            if (role == null) return RoleErrors.NotFound;

            var deleteResult = role.Delete();
            if (deleteResult.IsError) return deleteResult.Errors;

            var result = await roleManager.DeleteAsync(role);
            return result.Succeeded ? Result.Deleted : Error.Failure("Role.DeleteFailed");
        }
    }
}
