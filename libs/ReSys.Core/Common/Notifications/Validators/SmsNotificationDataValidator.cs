using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Validators;

public class SmsNotificationDataValidator : AbstractValidator<SmsNotificationData>
{
    public SmsNotificationDataValidator()
    {
        RuleFor(x => x.UseCase)
            .NotEqual(NotificationConstants.UseCase.None)
            .WithErrorCode("SmsNotification.UseCase.Missing")
            .WithMessage("Notification use case must be specified.");

        RuleFor(x => x.SendMethod)
            .Equal(NotificationConstants.SendMethod.SMS)
            .WithErrorCode("SmsNotification.SendMethod.Invalid")
            .WithMessage("SendMethod must be SMS for SmsNotificationData.");

        RuleFor(x => x.SenderNumber)
            .NotEmpty()
            .WithErrorCode("SmsNotification.SenderNumber.Missing")
            .WithMessage("Sender number is required for SMS notifications.");

        RuleFor(x => x.Receivers)
            .Must(r => r.Any(x => !string.IsNullOrWhiteSpace(x)))
            .WithErrorCode("SmsNotification.Receivers.Missing")
            .WithMessage("At least one valid phone number is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithErrorCode("SmsNotification.Content.Missing")
            .WithMessage("Content is required for SMS notifications.");

        When(x => !x.IsUnicode, () =>
        {
            RuleFor(x => x.Content)
                .MaximumLength(160)
                .WithErrorCode("SmsNotification.Content.TooLong")
                .WithMessage("SMS content exceeds 160 characters, which may be truncated by some carriers.");
        });
    }
}
