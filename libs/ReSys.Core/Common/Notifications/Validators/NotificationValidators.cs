using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Errors;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Validators;

public sealed class NotificationRecipientValidator : AbstractValidator<NotificationRecipient>
{
    public NotificationRecipientValidator()
    {
        // Default rules that don't depend on SendMethod
        RuleFor(x => x.Identifier).NotEmpty();
    }

    public NotificationRecipientValidator(NotificationConstants.SendMethod sendMethod)
    {
        Initialize(sendMethod);
    }

    public void Initialize(NotificationConstants.SendMethod sendMethod)
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
                .WithErrorCode(sendMethod == NotificationConstants.SendMethod.Email 
                    ? NotificationErrors.Email.ToRequired.Code 
                    : NotificationErrors.Sms.ToRequired.Code)
                .WithMessage(sendMethod == NotificationConstants.SendMethod.Email 
                    ? NotificationErrors.Email.ToRequired.Description 
                    : NotificationErrors.Sms.ToRequired.Description);

        if (sendMethod == NotificationConstants.SendMethod.Email)
        {
            RuleFor(x => x.Identifier)
                .EmailAddress()
                    .WithErrorCode(NotificationErrors.Email.InvalidFormat.Code)
                    .WithMessage(NotificationErrors.Email.InvalidFormat.Description);
        }
        else if (sendMethod == NotificationConstants.SendMethod.SMS)
        {
            RuleFor(x => x.Identifier)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                    .WithErrorCode(NotificationErrors.Sms.InvalidFormat.Code)
                    .WithMessage(NotificationErrors.Sms.InvalidFormat.Description);
        }
    }
}

public sealed class NotificationAttachmentValidator : AbstractValidator<NotificationAttachment>
{
    public NotificationAttachmentValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
                .WithErrorCode(NotificationErrors.Email.AttachmentFileNameRequired.Code)
                .WithMessage(NotificationErrors.Email.AttachmentFileNameRequired.Description);

        RuleFor(x => x.Data)
            .NotEmpty()
            .Must(x => x.Length <= NotificationConstants.Constraints.DefaultMaxAttachmentSizeInBytes)
                .WithErrorCode(NotificationErrors.Email.AttachmentTooLarge("file", NotificationConstants.Constraints.DefaultMaxAttachmentSizeInBytes).Code)
                .WithMessage(NotificationErrors.Email.AttachmentTooLarge("file", NotificationConstants.Constraints.DefaultMaxAttachmentSizeInBytes).Description);
    }
}

public sealed class NotificationMessageValidator : AbstractValidator<NotificationMessage>
{
    public NotificationMessageValidator()
    {
        RuleFor(x => x.UseCase)
            .NotEqual(NotificationConstants.UseCase.None)
                .WithErrorCode(NotificationErrors.General.UseCaseRequired.Code)
                .WithMessage(NotificationErrors.General.UseCaseRequired.Description);

        RuleFor(x => x.Recipient)
            .NotNull()
                .WithErrorCode(NotificationErrors.General.RecipientRequired.Code)
                .WithMessage(NotificationErrors.General.RecipientRequired.Description);

        RuleSet("Full", () => {
            RuleFor(x => x).Custom((message, context) => {
                if (!NotificationConstants.Templates.TryGetValue(message.UseCase, out var template))
                {
                    return;
                }

                var recipientValidator = new NotificationRecipientValidator(template.SendMethodType);
                var result = recipientValidator.Validate(message.Recipient);
                foreach (var error in result.Errors)
                {
                    context.AddFailure(error);
                }

                if (template.SendMethodType == NotificationConstants.SendMethod.Email && message.Attachments != null)
                {
                    var attachmentValidator = new NotificationAttachmentValidator();
                    foreach (var attachment in message.Attachments)
                    {
                        var attResult = attachmentValidator.Validate(attachment);
                        foreach (var error in attResult.Errors)
                        {
                            context.AddFailure(error);
                        }
                    }
                }
            });
        });
    }
}
