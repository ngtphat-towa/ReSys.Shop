using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Account.ResetPassword;

public static class ResetPassword
{
    public record Command(ResetPasswordRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Token).NotEmpty();
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
            var user = await _userManager.FindByEmailAsync(command.Request.Email);
            if (user == null)
            {
                return Result.Success; // Avoid revealing user existence
            }

            var result = await _userManager.ResetPasswordAsync(user, command.Request.Token, command.Request.NewPassword);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return Result.Success;
        }
    }
}
