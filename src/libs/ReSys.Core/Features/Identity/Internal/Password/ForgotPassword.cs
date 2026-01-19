using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using FluentValidation;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;
using ReSys.Core.Common.Notifications.Interfaces;

namespace ReSys.Core.Features.Identity.Internal.Password;

public static class ForgotPassword
{
    public record Request(string Email);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Find: user by email
            var user = await userManager.FindByEmailAsync(command.Request.Email);

            // Security Rule: Do not reveal if user exists
            if (user == null) return Result.Success;

            var result =  await userManager.GenerateAndSendPasswordResetCodeAsync(
                notificationService,
                configuration,
                user,
                cancellationToken);
                
            return Result.Success;
        }
    }
}