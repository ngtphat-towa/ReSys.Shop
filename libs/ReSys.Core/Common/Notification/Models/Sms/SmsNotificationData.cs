using ErrorOr;
using ReSys.Core.Feature.Common.Notification.Models.Notification;

namespace ReSys.Core.Feature.Common.Notification.Models.Sms;

public partial class SmsNotificationData : NotificationData
{
    public string SenderNumber { get; set; } = string.Empty;
    public bool IsUnicode { get; set; }

    public static ErrorOr<SmsNotificationData> Create(NotificationData notificationData)
    {
        var smsData = new SmsNotificationData
        {
             UseCase = notificationData.UseCase,
             SendMethodType = notificationData.SendMethodType,
             TemplateFormatType = notificationData.TemplateFormatType,
             Content = notificationData.Content,
             Title = notificationData.Title,
             Values = notificationData.Values,
             Receivers = notificationData.Receivers,
             Attachments = notificationData.Attachments,
             Priority = notificationData.Priority,
             Language = notificationData.Language,
             CreatedBy = notificationData.CreatedBy,
             CreatedAt = notificationData.CreatedAt
        };
        return smsData.Validate();
    }

    public new ErrorOr<SmsNotificationData> Validate()
    {
        var baseResult = base.Validate();
        if (baseResult.IsError) return baseResult.Errors;

        if (string.IsNullOrWhiteSpace(SenderNumber))
            return Errors.MissingSenderNumber;

        return this;
    }
}