using ErrorOr;
using ReSys.Core.Feature.Common.Notification.Models.Sms;

namespace ReSys.Core.Common.Notification.Interfaces;

public interface ISmsSenderService
{
    Task<ErrorOr<Success>> AddSmsNotificationAsync(
        SmsNotificationData notificationData,
        CancellationToken cancellationToken = default);
}
