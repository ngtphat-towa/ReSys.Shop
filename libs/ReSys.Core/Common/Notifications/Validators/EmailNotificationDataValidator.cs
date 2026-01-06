using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Validators;

public class EmailNotificationDataValidator : AbstractValidator<EmailNotificationData>
{
    public EmailNotificationDataValidator()
    {
        RuleFor(x => x.UseCase)
            .NotEqual(NotificationConstants.UseCase.None)
            .WithErrorCode("EmailNotification.UseCase.Missing")
            .WithMessage("Notification use case must be specified.");

        RuleFor(x => x.SendMethod)
            .Equal(NotificationConstants.SendMethod.Email)
            .WithErrorCode("EmailNotification.SendMethod.Invalid")
            .WithMessage("SendMethod must be Email for EmailNotificationData.");

        RuleFor(x => x.Receivers)
            .Must(r => r.Any(x => !string.IsNullOrWhiteSpace(x)))
            .WithErrorCode("EmailNotification.Receivers.Missing")
            .WithMessage("At least one valid email address is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode("EmailNotification.Title.Missing")
            .WithMessage("Title is required for email notifications.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.HtmlContent))
            .WithErrorCode("EmailNotification.Content.Missing")
            .WithMessage("At least one of Content or HtmlContent is required for email notifications.");
    }
}
