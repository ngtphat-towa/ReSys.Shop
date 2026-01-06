using ErrorOr;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Interfaces;

public interface ISmsSenderService
{
    public Task<ErrorOr<Success>> AddSmsNotificationAsync(
        SmsNotificationData notificationData, 
        CancellationToken cancellationToken = default);
}
