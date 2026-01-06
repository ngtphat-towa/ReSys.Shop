using ErrorOr;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class EmptyEmailSenderService : IEmailSenderService
{
    public Task<ErrorOr<Success>> SendAsync(
        NotificationRecipient to, 
        NotificationContent content, 
        NotificationMetadata metadata, 
        IEnumerable<NotificationAttachment>? attachments = null, 
        CancellationToken ct = default)
    {
        return Task.FromResult<ErrorOr<Success>>(
            Error.Unexpected(
                code: "EmailSender.Disabled",
                description: "Email sender is not available. Email sending is disabled in this environment."));
    }
}
