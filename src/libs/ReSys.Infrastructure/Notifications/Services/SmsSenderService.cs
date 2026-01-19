using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;
using Sinch;
using Sinch.SMS;
using Sinch.SMS.Batches;
using Sinch.SMS.Batches.Send;
using ReSys.Core.Common.Notifications.Errors;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class SmsSenderService(
    IOptions<SmsOptions> smsOption,
    ISinchClient sinchClient,
    ILogger<SmsSenderService> logger)
    : ISmsSenderService
{
    private readonly SmsOptions _smsOption = smsOption.Value;

    public async Task<ErrorOr<Success>> SendAsync(
        NotificationRecipient to,
        NotificationContent content,
        NotificationMetadata metadata,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Sending SMS via Sinch to {Recipient}", to.Identifier);

            ISinchSms smsApi = sinchClient.Sms;

            if (_smsOption.SinchConfig is null)
            {
                logger.LogError("Sinch configuration is missing.");
                return NotificationErrors.Sms.SinchConfigMissing;
            }

            var batchRequest = new SendTextBatchRequest
            {
                From = _smsOption.SinchConfig.SenderPhoneNumber,
                To = [to.Identifier],
                Body = content.Body
            };

            IBatch response = await smsApi.Batches.Send(batchRequest, ct);

            logger.LogInformation("Sinch SMS batch sent. BatchId: {BatchId}", response.Id);

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS via Sinch");
            return NotificationErrors.Sms.SendFailed(ex.Message);
        }
    }
}
