using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Common.Mailing;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Account.ForgotPassword;

public static class ForgotPassword
{
    public record Command(ForgotPasswordRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mailService;

        public Handler(UserManager<ApplicationUser> userManager, IMailService mailService)
        {
            _userManager = userManager;
            _mailService = mailService;
        }

        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.Email);
            if (user == null)
            {
                // Return success to avoid revealing user existence
                return Result.Success;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _mailService.SendEmailAsync(user.Email!, "Reset Password", $"Token: {token}");

            return Result.Success;
        }
    }
}
