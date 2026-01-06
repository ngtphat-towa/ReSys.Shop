using ErrorOr;

using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Interfaces;

public interface ISmsSenderService
{
    Task<ErrorOr<Success>> SendAsync(
        NotificationRecipient to,
        NotificationContent content,
        NotificationMetadata metadata,
        CancellationToken ct = default);
}
