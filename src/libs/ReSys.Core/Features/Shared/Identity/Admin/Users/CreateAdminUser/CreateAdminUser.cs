using MediatR;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Mapster;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Admin.Users.Common;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.CreateAdminUser;

public static class CreateAdminUser
{
    public record Request : AccountParameters
    {
        public string Password { get; init; } = null!;
        public List<string> Roles { get; init; } = [];
    }

    public record Response : AdminUserDetailResponse;

    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AccountValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;

            // 1. Uniqueness check
            if (await userManager.FindByEmailAsync(req.Email) != null)
                return UserErrors.EmailAlreadyExists(req.Email);

            // 2. Aggregate creation
            var userResult = User.Create(req.Email, req.UserName, req.FirstName, req.LastName);
            if (userResult.IsError) return userResult.Errors;
            var user = userResult.Value;

            // 3. Persistence
            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User");

            // 4. Role assignment (optional)
            if (req.Roles.Any())
            {
                await userManager.AddToRolesAsync(user, req.Roles);
            }

            return user.Adapt<Response>();
        }
    }
}
