using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

/// <summary>
/// Represents the data required to send an SMS notification.
/// Includes recipients, content, metadata, and parameters for template replacement.
/// </summary>
public partial class SmsNotificationData
{
    public required NotificationConstants.UseCase UseCase { get; set; }
    public List<string> Receivers { get; set; } = [];
    public string Content { get; set; } = string.Empty;
    public string SenderNumber { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "System";
    public DateTimeOffset? CreatedAt { get; set; }
    public NotificationConstants.PriorityLevel Priority { get; set; } = NotificationConstants.PriorityLevel.Normal;
    public NotificationConstants.SendMethod SendMethod { get; set; } = NotificationConstants.SendMethod.SMS;
    public string Language { get; set; } = "en-US";
    public bool IsUnicode { get; set; }
    public string? TrackingId { get; set; }
}