using System.Net.Mail;

using ErrorOr;

using FluentEmail.Core;
using FluentEmail.Core.Models;

using FluentValidation;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Infrastructure.Notifications.Options;

using Attachment = FluentEmail.Core.Models.Attachment;

namespace ReSys.Infrastructure.Notifications.Services;

public sealed class EmailSenderService(
    IOptions<SmtpOptions> emailSettings,
    IFluentEmail fluentEmail,
    ILogger<EmailSenderService> logger,
    IValidator<EmailNotificationData> validator)
    : IEmailSenderService
{
    private readonly SmtpOptions _emailOption = emailSettings.Value;

    public async Task<ErrorOr<Success>> AddEmailNotificationAsync(EmailNotificationData notificationData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(notificationData, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .ConvertAll(validationFailure => Error.Validation(
                        code: validationFailure.ErrorCode,
                        description: validationFailure.ErrorMessage));
                return errors;
            }

            if (notificationData.Attachments.Count != 0)
            {
                int maxSizeInBytes = _emailOption.MaxAttachmentSize ?? 25 * 1024 * 1024; // Default to 25MB
                List<string> missingAttachments = notificationData.Attachments.Where(a => !File.Exists(a)).ToList();
                if (missingAttachments.Count != 0)
                    return Errors.InvalidAttachments(missingAttachments);

                foreach (string attachment in notificationData.Attachments)
                {
                    FileInfo fileInfo = new(attachment);
                    if (fileInfo.Length > maxSizeInBytes)
                        return Errors.AttachmentSize(attachment, maxSizeInBytes);
                }
            }

            var email = fluentEmail
                .SetFrom(_emailOption.FromEmail, _emailOption.FromName)
                .To(notificationData.Receivers.Select(m => new Address(m)))
                .Subject(notificationData.Title)
                .PlaintextAlternativeBody(notificationData.Content)
                .Body(notificationData.HtmlContent, true);

            foreach (var cc in notificationData.Cc)
            {
                email.CC(cc);
            }

            foreach (var bcc in notificationData.Bcc)
            {
                email.BCC(bcc);
            }

            if (notificationData.Attachments.Count != 0)
            {
                FileExtensionContentTypeProvider contentTypeProvider = new();
                foreach (string attachmentPath in notificationData.Attachments)
                {
                    byte[] attachmentBytes = await File.ReadAllBytesAsync(attachmentPath, cancellationToken);
                    email.Attach(new Attachment
                    {
                        Filename = Path.GetFileName(attachmentPath),
                        Data = new MemoryStream(attachmentBytes),
                        ContentType = contentTypeProvider.TryGetContentType(attachmentPath, out string? contentType)
                            ? contentType
                            : "application/octet-stream"
                    });
                }
            }

            logger.LogInformation(
                "Sending email notification with UseCase: {UseCase}, Priority: {Priority}, Language: {Language} to {Receivers}",
                notificationData.UseCase,
                notificationData.Priority,
                notificationData.Language,
                string.Join(", ", notificationData.Receivers));

            SendResponse? sendResult = await email.SendAsync(cancellationToken);
            if (!sendResult.Successful)
            {
                logger.LogError("Failed to send email notification. Errors: {Errors}", string.Join(", ", sendResult.ErrorMessages));
                return Errors.SendFailed(sendResult.ErrorMessages);
            }

            logger.LogInformation("Email notification sent successfully to {Receivers}", string.Join(", ", notificationData.Receivers));
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while sending email notification.");
            return Errors.ExceptionOccurred(ex);
        }
    }

    public static class Errors
    {
        public static Error InvalidAttachments(List<string> missingAttachments) => Error.Validation(
            code: "EmailNotification.InvalidAttachments",
            description: $"The following attachments were not found: {string.Join(", ", missingAttachments)}");

        public static Error AttachmentSize(string attachment, long maxSizeInBytes) => Error.Validation(
            code: "EmailNotification.AttachmentSize",
            description: $"Attachment {attachment} exceeds the maximum size of {maxSizeInBytes / 1024 / 1024}MB.");

        public static Error SendFailed(IList<string> errorMessages) => Error.Unexpected(
            code: "EmailNotification.SendFailed",
            description: $"Failed to send email: {string.Join(", ", errorMessages)}");

        public static Error ExceptionOccurred(Exception ex) => Error.Unexpected(
            code: "EmailNotification.ExceptionOccurred",
            description: $"An exception occurred while sending email: {ex.Message}");
    }
}
