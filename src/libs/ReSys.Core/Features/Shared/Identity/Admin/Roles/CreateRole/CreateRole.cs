using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Shared.Identity.Admin.Roles.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using FluentValidation;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.CreateRole;

public static class CreateRole
{
    public record Request : RoleParameters;
    public record Response : RoleResponse;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty();
        }
    }

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;

            if (await roleManager.RoleExistsAsync(req.Name))
                return RoleErrors.DuplicateName(req.Name);

            var roleResult = Role.Create(req.Name, req.DisplayName, req.Description, req.Priority);
            if (roleResult.IsError) return roleResult.Errors;
            
            var role = roleResult.Value;
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "Role");

            return role.Adapt<Response>();
        }
    }
}
