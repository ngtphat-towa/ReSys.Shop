using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Shared.Identity.Internal.Confirmations;

public static class ResendEmailConfirmation
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
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.FindByEmailAsync(command.Request.Email);
            
            // Security: Always return success to prevent email enumeration
            if (user == null || user.EmailConfirmed) return Result.Success;

            // Trigger Notification
            return await userManager.GenerateAndSendConfirmationEmailAsync(
                notificationService, configuration, user, cancellationToken: ct);
        }
    }
}
