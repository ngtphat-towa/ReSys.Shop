using ErrorOr;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Interfaces;

public interface INotificationService
{
    Task<ErrorOr<Success>> AddNotificationAsync(NotificationData notification, CancellationToken cancellationToken);
}
