using ErrorOr;
using ReSys.Core.Feature.Common.Notification.Models.Emails;

namespace ReSys.Core.Common.Notification.Interfaces;

public interface IEmailSenderService
{
    Task<ErrorOr<Success>> AddEmailNotificationAsync(
        EmailNotificationData notificationData,
        CancellationToken cancellationToken = default);
}