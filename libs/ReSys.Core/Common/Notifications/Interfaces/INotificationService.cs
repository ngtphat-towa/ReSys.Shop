using ErrorOr;

using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Unified send method using the NotificationMessage model.
    /// </summary>
    Task<ErrorOr<Success>> SendAsync(NotificationMessage message, CancellationToken ct = default);
}
