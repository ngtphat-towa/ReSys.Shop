using ErrorOr;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class EmptySmsSenderService : ISmsSenderService
{
    public Task<ErrorOr<Success>> AddSmsNotificationAsync(
        SmsNotificationData notificationData,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ErrorOr<Success>>(
            Error.Unexpected(
                code: "SmsNotification.Unavailable",
                description: "SMS sending is currently unavailable or disabled."));
    }
}
