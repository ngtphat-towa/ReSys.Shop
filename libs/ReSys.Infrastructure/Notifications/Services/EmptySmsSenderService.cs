using ErrorOr;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class EmptySmsSenderService : ISmsSenderService
{
    public Task<ErrorOr<Success>> SendAsync(
        NotificationRecipient to, 
        NotificationContent content, 
        NotificationMetadata metadata, 
        CancellationToken ct = default)
    {
        return Task.FromResult<ErrorOr<Success>>(
            Error.Unexpected(
                code: "SmsNotification.Unavailable",
                description: "SMS sending is currently unavailable or disabled."));
    }
}
