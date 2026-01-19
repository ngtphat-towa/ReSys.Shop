using MediatR;

using Microsoft.AspNetCore.Identity;

using FluentValidation;

using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Internal.Password;

public static class ChangePassword
{
    // Request:
    public record Request(string CurrentPassword, string NewPassword);

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.CurrentPassword).NotEmpty();
            RuleFor(x => x.Request.NewPassword).NotEmpty().MinimumLength(8);
        }
    }

    // Handler:
    public class Handler(
        UserManager<User> userManager,
        IUserContext userContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Authenticated user
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            // Fetch: users
            var user = await userManager.FindByIdAsync(userContext.UserId);
            if (user == null) return UserErrors.NotFound(userContext.UserId);

            // Change: password
            var result = await userManager.ChangePasswordAsync(
                user,
                command.Request.CurrentPassword,
                command.Request.NewPassword);

            if (!result.Succeeded)
                return result.Errors.ToApplicationResult(prefix: "ChangePassword");

            return Result.Success;
        }
    }
}