using ErrorOr;
using System.Text.RegularExpressions;
using ReSys.Core.Feature.Common.Notification.Constants;

namespace ReSys.Core.Feature.Common.Notification.Models.Notification;

public partial class NotificationData
{
    public NotificationConstants.UseCase UseCase { get; set; }
    public NotificationConstants.SendMethod SendMethodType { get; set; }
    public NotificationFormats.Enumerate TemplateFormatType { get; set; }
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
    public string? Title { get; set; }
    public Dictionary<NotificationConstants.Parameter, string?> Values { get; set; } = new();
    public List<string> Receivers { get; set; } = new();
    public List<string> Attachments { get; set; } = new();
    public NotificationConstants.PriorityLevel Priority { get; set; }
    public string? Language { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Sender { get; set; }

    public ErrorOr<NotificationData> Validate()
    {
        List<Error> errors = new();

        if (UseCase == NotificationConstants.UseCase.None)
            errors.Add(Errors.MissingUseCase);

        if (!Receivers.Any(r => !string.IsNullOrWhiteSpace(r)))
            errors.Add(Errors.MissingReceiver);

        if (string.IsNullOrWhiteSpace(CreatedBy))
            errors.Add(Errors.EmptyCreatedBy);

        if (SendMethodType == NotificationConstants.SendMethod.Email)
        {
            if (string.IsNullOrWhiteSpace(Title))
                errors.Add(Errors.MissingEmailTitle);

            if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(HtmlContent))
                errors.Add(Errors.MissingEmailContent);
        }
        else if (SendMethodType == NotificationConstants.SendMethod.SMS)
        {
            if (string.IsNullOrWhiteSpace(Content))
                errors.Add(Errors.MissingSmsContent);
            else if (Content.Length > 160)
                errors.Add(Errors.SmsContentTooLong);
        }

        if (errors.Count > 0)
            return errors;

        return this;
    }
}