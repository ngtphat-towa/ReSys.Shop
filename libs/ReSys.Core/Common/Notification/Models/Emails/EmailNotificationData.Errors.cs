using ErrorOr;

namespace ReSys.Core.Feature.Common.Notification.Models.Emails;

public partial class EmailNotificationData
{
    public new static class Errors
    {
        public static Error MissingUseCase => Error.Validation(
            code: "EmailNotificationData.MissingUseCase",
            description: "Notification use case is required.");

        public static Error InvalidSendMethod => Error.Validation(
            code: "EmailNotificationData.InvalidSendMethod",
            description: "Invalid send method for email notification.");

        public static Error MissingReceivers => Error.Validation(
            code: "EmailNotificationData.MissingReceivers",
            description: "At least one receiver is required.");

        public static Error MissingTitle => Error.Validation(
            code: "EmailNotificationData.MissingTitle",
            description: "Email title is required.");

        public static Error MissingContent => Error.Validation(
            code: "EmailNotificationData.MissingContent",
            description: "Email content (text or HTML) is required.");
    }
}
