using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

/// <summary>
/// A high-level request object for sending a notification.
/// </summary>
public sealed record NotificationMessage
{
    public NotificationConstants.UseCase UseCase { get; init; }
    public NotificationRecipient Recipient { get; init; }
    public NotificationContext Context { get; init; }
    public List<NotificationAttachment>? Attachments { get; init; }
    public NotificationMetadata Metadata { get; init; }

    internal NotificationMessage(
        NotificationConstants.UseCase useCase,
        NotificationRecipient recipient,
        NotificationContext context,
        List<NotificationAttachment>? attachments = null,
        NotificationMetadata? metadata = null)
    {
        UseCase = useCase;
        Recipient = recipient;
        Context = context;
        Attachments = attachments;
        Metadata = metadata ?? NotificationMetadata.Default;
    }
}
