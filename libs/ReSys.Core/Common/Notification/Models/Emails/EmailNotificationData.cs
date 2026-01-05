using ErrorOr;
using ReSys.Core.Feature.Common.Notification.Models.Notification;

namespace ReSys.Core.Feature.Common.Notification.Models.Emails;

public partial class EmailNotificationData : NotificationData
{
    public static ErrorOr<EmailNotificationData> Create(
        NotificationData notificationData,
        string? emailTo,
        string? emailSubject,
        string? emailBody,
        string? emailHtmlBody)
    {
        var emailData = new EmailNotificationData
        {
            UseCase = notificationData.UseCase,
            SendMethodType = notificationData.SendMethodType,
            TemplateFormatType = notificationData.TemplateFormatType,
            Content = notificationData.Content,
            HtmlContent = notificationData.HtmlContent,
            Title = notificationData.Title,
            Values = notificationData.Values,
            Receivers = notificationData.Receivers,
            Attachments = notificationData.Attachments,
            Priority = notificationData.Priority,
            Language = notificationData.Language,
            CreatedBy = notificationData.CreatedBy,
            CreatedAt = notificationData.CreatedAt
        };
        
        return emailData.Validate();
    }

    public new ErrorOr<EmailNotificationData> Validate()
    {
        var baseResult = base.Validate();
        if (baseResult.IsError) return baseResult.Errors;

        // Additional Email specific validation if needed
        return this;
    }
}