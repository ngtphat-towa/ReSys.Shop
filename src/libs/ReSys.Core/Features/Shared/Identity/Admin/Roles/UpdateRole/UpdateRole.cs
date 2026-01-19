using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Shared.Identity.Admin.Roles.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.UpdateRole;

public static class UpdateRole
{
    public record Request : RoleParameters;
    public record Response : RoleResponse;
    public record Command(string Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var role = await roleManager.FindByIdAsync(command.Id);
            if (role == null) return RoleErrors.NotFound;

            var req = command.Request;
            var updateResult = role.Update(req.DisplayName, req.Description, req.Priority);
            if (updateResult.IsError) return updateResult.Errors;

            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "Role");

            return role.Adapt<Response>();
        }
    }
}
