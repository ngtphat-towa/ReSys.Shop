using ErrorOr;

using ReSys.Core.Common.Notifications.Constants;

namespace ReSys.Core.Common.Notifications.Errors;

public static class NotificationErrors
{
    public static class Email
    {
        public static Error ToRequired => Error.Validation(
            code: "Notification.Email.ToRequired",
            description: "At least one recipient is required.");

        public static Error InvalidFormat => Error.Validation(
            code: "Notification.Email.InvalidFormat",
            description: "Invalid email format.");

        public static Error SubjectRequired => Error.Validation(
            code: "Notification.Email.SubjectRequired",
            description: "Subject is required.");

        public static Error ContentRequired => Error.Validation(
            code: "Notification.Email.ContentRequired",
            description: "Either Body or HtmlBody must be provided.");

        public static Error SendFailed(string details) => Error.Failure(
            code: "Notification.Email.SendFailed",
            description: $"Failed to send email: {details}");

        public static Error SmtpConfigMissing => Error.Unexpected(
            code: "Notification.Email.SmtpConfigMissing",
            description: "SMTP configuration is missing.");

        public static Error AttachmentNotFound(string path) => Error.Validation(
            code: "Notification.Email.AttachmentNotFound",
            description: $"Attachment not found: {path}");

        public static Error AttachmentTooLarge(string path, int maxSize) => Error.Validation(
            code: "Notification.Email.AttachmentTooLarge",
            description: $"Attachment {path} exceeds maximum size of {maxSize / 1024 / 1024}MB.");

        public static Error AttachmentFileNameRequired => Error.Validation(
            code: "Notification.Email.AttachmentFileNameRequired",
            description: "Attachment file name is required.");
    }

    public static class Sms
    {
        public static Error ToRequired => Error.Validation(
            code: "Notification.Sms.ToRequired",
            description: "At least one recipient is required.");

        public static Error InvalidFormat => Error.Validation(
            code: "Notification.Sms.InvalidFormat",
            description: "Invalid phone number format.");

        public static Error BodyRequired => Error.Validation(
            code: "Notification.Sms.BodyRequired",
            description: "SMS body is required.");

        public static Error BodyTooLong => Error.Validation(
            code: "Notification.Sms.BodyTooLong",
            description: $"SMS body cannot exceed {NotificationConstants.Constraints.SmsMaxLength} characters.");

        public static Error SendFailed(string details) => Error.Failure(
            code: "Notification.Sms.SendFailed",
            description: $"Failed to send SMS: {details}");

        public static Error SinchConfigMissing => Error.Unexpected(
            code: "Notification.Sms.SinchConfigMissing",
            description: "Sinch configuration is missing.");
    }

    public static class General
    {
        public static Error TemplateNotFound(string useCase) => Error.NotFound(
            code: "Notification.TemplateNotFound",
            description: $"Template for use case '{useCase}' not found.");

        public static Error UnsupportedMethod => Error.Validation(
            code: "Notification.UnsupportedMethod",
            description: "The template specifies an unsupported send method.");

        public static Error Unexpected(string message) => Error.Unexpected(
            code: "Notification.Unexpected",
            description: message);

        public static Error UseCaseRequired => Error.Validation(
            code: "Notification.UseCaseRequired",
            description: "UseCase is required.");

        public static Error RecipientRequired => Error.Validation(
            code: "Notification.RecipientRequired",
            description: "Recipient is required.");
    }
}