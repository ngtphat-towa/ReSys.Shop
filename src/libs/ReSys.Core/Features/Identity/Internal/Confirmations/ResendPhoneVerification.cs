using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Common;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Identity.Internal.Confirmations;

public static class ResendPhoneVerification
{
    public record Request(string PhoneNumber);
    public record Command(Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.PhoneNumber).NotEmpty();
        }
    }

    public class Handler(
        UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            // Note: Phone lookups are usually authenticated or require an active session
            // For this slice, we assume lookup by phone number
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == command.Request.PhoneNumber, ct);
            
            if (user == null || user.PhoneNumberConfirmed) return Result.Success;

            // Trigger SMS Notification
            return await userManager.GenerateAndSendConfirmationSmsAsync(
                notificationService, configuration, user, cancellationToken: ct);
        }
    }
}