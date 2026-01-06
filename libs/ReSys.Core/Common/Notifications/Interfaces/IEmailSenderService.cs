using ErrorOr;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Interfaces;

public interface IEmailSenderService
{
    Task<ErrorOr<Success>> AddEmailNotificationAsync(
        EmailNotificationData notificationData,
        CancellationToken cancellationToken = default);
}
