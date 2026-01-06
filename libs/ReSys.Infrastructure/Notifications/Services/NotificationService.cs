using ErrorOr;
using FluentValidation;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class NotificationService(
    IEmailSenderService emailSenderService,
    ISmsSenderService smsSenderService,
    IValidator<NotificationData> validator)
    : INotificationService
{
    public async Task<ErrorOr<Success>> AddNotificationAsync(
        NotificationData notificationData,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await validator.ValidateAsync(notificationData, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .ConvertAll(validationFailure => Error.Validation(
                    code: validationFailure.ErrorCode,
                    description: validationFailure.ErrorMessage));
            return errors;
        }

        return await DispatchNotificationAsync(notificationData, cancellationToken);
    }

    private async Task<ErrorOr<Success>> DispatchNotificationAsync(
        NotificationData notificationData,
        CancellationToken cancellationToken)
    {
        return notificationData.SendMethodType switch
        {
            NotificationConstants.SendMethod.Email => await emailSenderService.AddEmailNotificationAsync(
                notificationData.ToEmailNotificationData(),
                cancellationToken
            ),

            NotificationConstants.SendMethod.SMS => await smsSenderService.AddSmsNotificationAsync(
                notificationData.ToSmsNotificationData(),
                cancellationToken
            ),

            _ => Errors.NotSupportedSendMethod
        };
    }

    public static class Errors
    {
        public static Error NotSupportedSendMethod => Error.Validation(
            code: "NotificationService.NotSupportedSendMethod",
            description: "The specified send method type is not supported.");
    }
}