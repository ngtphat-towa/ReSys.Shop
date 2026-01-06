using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

/// <summary>
/// Represents the data required to send an email notification.
/// Includes recipients, content, attachments, and metadata for context.
/// </summary>
public partial class EmailNotificationData
{
    public required NotificationConstants.UseCase UseCase { get; set; }
    public List<string> Receivers { get; set; } = [];
    public List<string> Cc { get; set; } = [];
    public List<string> Bcc { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
    public string CreatedBy { get; set; } = "System";
    public List<string> Attachments { get; set; } = [];
    public DateTimeOffset? CreatedAt { get; set; }
    public NotificationConstants.PriorityLevel Priority { get; set; } = NotificationConstants.PriorityLevel.Normal;
    public NotificationConstants.SendMethod SendMethod { get; set; } = NotificationConstants.SendMethod.Email;
    public string Language { get; set; } = "en-US";
}