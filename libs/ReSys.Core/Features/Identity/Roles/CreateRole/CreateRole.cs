using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Roles.CreateRole;

public static class CreateRole
{
    public record Command(CreateRoleRequest Request) : IRequest<ErrorOr<string>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().MinimumLength(3);
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<string>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public Handler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ErrorOr<string>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (await _roleManager.RoleExistsAsync(request.Name))
            {
                return IdentityErrors.RoleExists;
            }

            var role = new ApplicationRole(request.Name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return role.Id;
        }
    }
}
