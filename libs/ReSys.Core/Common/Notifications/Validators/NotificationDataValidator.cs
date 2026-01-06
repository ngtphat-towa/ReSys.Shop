using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;
using System.Text.RegularExpressions;

namespace ReSys.Core.Common.Notifications.Validators;

public class NotificationDataValidator : AbstractValidator<NotificationData>
{
    public NotificationDataValidator()
    {
        RuleFor(x => x.UseCase)
            .NotEqual(NotificationConstants.UseCase.None)
            .WithErrorCode("Notification.UseCase.Missing")
            .WithMessage("Notification use case must be specified.");

        RuleFor(x => x.Receivers)
            .Must(r => r.Any(x => !string.IsNullOrWhiteSpace(x)))
            .WithErrorCode("Notification.Receivers.Missing")
            .WithMessage("At least one valid receiver is required.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithErrorCode("Notification.CreatedBy.Missing")
            .WithMessage("CreatedBy cannot be empty or whitespace.");

        When(x => x.SendMethodType == NotificationConstants.SendMethod.Email, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithErrorCode("Notification.Email.Title.Missing")
                .WithMessage("Title is required for Email notifications.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.HtmlContent))
                .WithErrorCode("Notification.Email.Content.Missing")
                .WithMessage("At least one of Content or HtmlContent is required for Email notifications.");

            RuleFor(x => x.Sender)
                .Must(sender => string.IsNullOrWhiteSpace(sender) || Regex.IsMatch(sender, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                .WithErrorCode("Notification.Email.Sender.Invalid")
                .WithMessage("Sender must be a valid email address.");
        });

        When(x => x.SendMethodType == NotificationConstants.SendMethod.SMS, () =>
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithErrorCode("Notification.SMS.Content.Missing")
                .WithMessage("Content is required for SMS notifications.");
            
            RuleFor(x => x.Content)
                .MaximumLength(160)
                .WithErrorCode("Notification.SMS.Content.TooLong")
                .WithMessage("SMS content exceeds 160 characters and may be truncated.");

            RuleFor(x => x.Sender)
                .Must(sender => string.IsNullOrWhiteSpace(sender) || Regex.IsMatch(sender, @"^\+?[1-9]\d{7,14}$"))
                .WithErrorCode("Notification.SMS.Sender.Invalid")
                .WithMessage("Sender must be a valid phone number.");
        });
    }
}
