using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Common.Notifications.Builders;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Account;

public static class ChangeEmail
{
    public record Request(string NewEmail);

    public record Command(string UserId, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.NewEmail).NotEmpty().EmailAddress();
        }
    }

    public class Handler(UserManager<ApplicationUser> userManager, INotificationService notificationService) 
        : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("User.NotFound", "User not found");

            if (user.Email == request.Data.NewEmail)
            {
                return Error.Validation("Email.Same", "New email is same as current email");
            }

            var token = await userManager.GenerateChangeEmailTokenAsync(user, request.Data.NewEmail);
            
            // Send notification using established system
            var message = NotificationMessageBuilder
                .ForUseCase(NotificationConstants.UseCase.SystemAccountUpdate)
                .To(NotificationRecipient.Create(request.Data.NewEmail))
                .AddParam(NotificationConstants.Parameter.OtpCode, token)
                .AddParam(NotificationConstants.Parameter.UserFirstName, user.UserName ?? "User");

            if (message.IsError) return message.Errors;

            await notificationService.SendAsync(message.Value, cancellationToken);

            return Result.Success;
        }
    }
}