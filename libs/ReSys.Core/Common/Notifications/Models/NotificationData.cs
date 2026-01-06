using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Models;

/// <summary>
/// Represents the base model for sending notifications, regardless of delivery method (Email, SMS, Push, etc.).
/// Contains common metadata, content, and personalization values used in specific notification types.
/// </summary>
public partial class NotificationData
{
    public required NotificationConstants.UseCase UseCase { get; set; }
    public NotificationConstants.SendMethod SendMethodType { get; set; } = NotificationConstants.SendMethod.Email;
    public NotificationFormats.Enumerate TemplateFormatType { get; set; } = NotificationFormats.Enumerate.Default;
    public Dictionary<NotificationConstants.Parameter, string?> Values { get; set; } = new();
    public List<string> Receivers { get; set; } = [];
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTimeOffset? CreatedAt { get; set; }
    public List<string> Attachments { get; set; } = [];
    public NotificationConstants.PriorityLevel Priority { get; set; } = NotificationConstants.PriorityLevel.Normal;
    public string Language { get; set; } = "en-US";
    public string? Sender { get; set; }
}