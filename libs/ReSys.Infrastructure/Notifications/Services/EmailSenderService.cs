using ErrorOr;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;
using Attachment = FluentEmail.Core.Models.Attachment;
using ReSys.Core.Common.Notifications.Errors;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class EmailSenderService(
    IOptions<SmtpOptions> emailSettings,
    IFluentEmail fluentEmail,
    ILogger<EmailSenderService> logger)
    : IEmailSenderService
{
    private readonly SmtpOptions _emailOption = emailSettings.Value;

    public async Task<ErrorOr<Success>> SendAsync(
        NotificationRecipient to,
        NotificationContent content,
        NotificationMetadata metadata,
        IEnumerable<NotificationAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        try
        {
            var email = fluentEmail
                .SetFrom(_emailOption.FromEmail, _emailOption.FromName)
                .To(to.Identifier, to.Name)
                .Subject(content.Subject)
                .PlaintextAlternativeBody(content.Body)
                .Body(content.HtmlBody, true);

            var attachmentsList = attachments?.ToList() ?? [];
            if (attachmentsList.Count != 0)
            {
                foreach (var attachment in attachmentsList)
                {
                    email.Attach(new Attachment
                    {
                        Filename = attachment.FileName,
                        Data = new MemoryStream(attachment.Data),
                        ContentType = attachment.ContentType
                    });
                }
            }

            logger.LogInformation(
                "Sending email notification. Priority: {Priority}, Language: {Language} to {Recipient}",
                metadata.Priority,
                metadata.Language,
                to.Identifier);

            SendResponse? sendResult = await email.SendAsync(ct);
            if (!sendResult.Successful)
            {
                logger.LogError("Failed to send email notification. Errors: {Errors}", string.Join(", ", sendResult.ErrorMessages));
                return NotificationErrors.Email.SendFailed(string.Join(", ", sendResult.ErrorMessages));
            }

            logger.LogInformation("Email notification sent successfully to {Recipient}", to.Identifier);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while sending email notification.");
            return NotificationErrors.General.Unexpected(ex.Message);
        }
    }
}