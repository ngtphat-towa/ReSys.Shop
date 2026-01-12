using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ReSys.Identity.Features.Management.Roles;

public static class Create
{
    public record Request(string Name);

    public record Command(Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Name).NotEmpty();
        }
    }

    public class Handler(RoleManager<IdentityRole> roleManager) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await roleManager.RoleExistsAsync(request.Data.Name))
            {
                return Error.Conflict("Roles.Duplicate", "Role already exists");
            }

            var result = await roleManager.CreateAsync(new IdentityRole(request.Data.Name));
            
            if (result.Succeeded) return Result.Success;

            return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }
    }
}
