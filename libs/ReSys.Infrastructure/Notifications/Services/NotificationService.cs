using ErrorOr;
using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Notifications.Errors;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class NotificationService(
    IEmailSenderService emailSenderService,
    ISmsSenderService smsSenderService,
    IValidator<NotificationMessage> validator)
    : INotificationService
{
    public async Task<ErrorOr<Success>> SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(message, options => options.IncludeRuleSets("Full"), ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))
                .ToList();
            return errors;
        }

        if (!NotificationConstants.Templates.TryGetValue(message.UseCase, out var template))
        {
            return NotificationErrors.General.TemplateNotFound(message.UseCase.ToString());
        }

        var contentResult = message.MapContent();
        if (contentResult.IsError) return contentResult.Errors;

        return template.SendMethodType switch
        {
            NotificationConstants.SendMethod.Email => await emailSenderService.SendAsync(
                message.Recipient, 
                contentResult.Value, 
                message.Metadata, 
                message.Attachments, 
                ct),

            NotificationConstants.SendMethod.SMS => await smsSenderService.SendAsync(
                message.Recipient, 
                contentResult.Value, 
                message.Metadata, 
                ct),

            _ => NotificationErrors.General.UnsupportedMethod
        };
    }
}
