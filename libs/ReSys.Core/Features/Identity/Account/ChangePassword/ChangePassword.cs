using System.Security.Claims;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Common;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Account.ChangePassword;

public static class ChangePassword
{
    public record Command(ClaimsPrincipal User, ChangePasswordRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.CurrentPassword).NotEmpty();
            RuleFor(x => x.Request.NewPassword).NotEmpty().MinimumLength(6);
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(command.User);
            if (user == null) return IdentityErrors.UserNotFound;

            var result = await _userManager.ChangePasswordAsync(user, command.Request.CurrentPassword, command.Request.NewPassword);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return Result.Success;
        }
    }
}
