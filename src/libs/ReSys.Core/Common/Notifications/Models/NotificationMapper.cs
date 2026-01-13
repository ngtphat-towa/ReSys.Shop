using System.Text.RegularExpressions;

using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Errors;

using ErrorOr;

namespace ReSys.Core.Common.Notifications.Models;

public static class NotificationMapper
{
    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    public static ErrorOr<NotificationContent> MapContent(this NotificationMessage message)
    {
        if (!NotificationConstants.Templates.TryGetValue(message.UseCase, out var template))
        {
            return NotificationErrors.General.TemplateNotFound(message.UseCase.ToString());
        }

        var subject = FillTemplate(template.Name, message.Context);
        var body = FillTemplate(template.TemplateContent ?? string.Empty, message.Context);
        var htmlBody = FillTemplate(template.HtmlTemplateContent ?? string.Empty, message.Context);

        return NotificationContent.Create(subject, body, string.IsNullOrWhiteSpace(htmlBody) ? null : htmlBody);
    }

    private static string FillTemplate(string template, NotificationContext context)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;

        return PlaceholderRegex.Replace(template, match =>
        {
            var keyName = match.Groups[1].Value;
            if (Enum.TryParse<NotificationConstants.Parameter>(keyName, out var param))
            {
                return context.GetValue(param) ?? match.Value;
            }
            return match.Value;
        });
    }
}
