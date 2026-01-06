using System.Text.RegularExpressions;
using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;
using Sinch;
using Sinch.SMS;
using Sinch.SMS.Batches;
using Sinch.SMS.Batches.Send;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class SmsSenderService(
    IOptions<SmsOptions> smsOption,
    ISinchClient sinchClient,
    ILogger<SmsSenderService> logger,
    IValidator<SmsNotificationData> validator)
    : ISmsSenderService
{
    private readonly SmsOptions _smsOption = smsOption.Value;

    public async Task<ErrorOr<Success>> AddSmsNotificationAsync(
        SmsNotificationData notificationData,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(notificationData, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .ConvertAll(validationFailure => Error.Validation(
                    code: validationFailure.ErrorCode,
                    description: validationFailure.ErrorMessage));
            return errors;
        }

        try
        {
            logger.LogInformation("Sending SMS via Sinch to {Receivers} with UseCase {UseCase}",
                string.Join(", ", notificationData.Receivers),
                notificationData.UseCase);

            ISinchSms smsApi = sinchClient.Sms;

            // Ensure SinchConfig is present (SmsOptions validation should catch this, but safe to check)
            if (_smsOption.SinchConfig is null)
            {
                 logger.LogError("Sinch configuration is missing.");
                 return Error.Unexpected("SmsNotification.ConfigMissing", "Sinch configuration is missing.");
            }

            var batchRequest = new SendTextBatchRequest
            {
                From = string.IsNullOrWhiteSpace(notificationData.SenderNumber)
                        ? _smsOption.SinchConfig.SenderPhoneNumber
                        : notificationData.SenderNumber,
                To = notificationData.Receivers,
                Body = notificationData.Content
            };

            IBatch response = await smsApi.Batches.Send(batchRequest, cancellationToken);

            logger.LogInformation("Sinch SMS batch sent. BatchId: {BatchId}", response.Id);

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS via Sinch");
            return Error.Failure(code: "SmsNotification.Failed",
                description: $"Failed to send SMS: {ex.Message}");
        }
    }
}
