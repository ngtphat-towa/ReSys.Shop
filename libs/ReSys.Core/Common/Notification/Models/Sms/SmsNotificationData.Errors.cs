using ErrorOr;

namespace ReSys.Core.Feature.Common.Notification.Models.Sms;

public partial class SmsNotificationData
{
    public new static class Errors
    {
        public static Error MissingUseCase => Error.Validation(
            code: "SmsNotificationData.MissingUseCase",
            description: "Notification use case is required.");

        public static Error InvalidSendMethod => Error.Validation(
            code: "SmsNotificationData.InvalidSendMethod",
            description: "Invalid send method for SMS notification.");

        public static Error MissingSenderNumber => Error.Validation(
            code: "SmsNotificationData.MissingSenderNumber",
            description: "Sender number is required.");

        public static Error MissingReceivers => Error.Validation(
            code: "SmsNotificationData.MissingReceivers",
            description: "At least one receiver is required.");

        public static Error MissingContent => Error.Validation(
            code: "SmsNotificationData.MissingContent",
            description: "SMS content is required.");

        public static Error ContentTooLong => Error.Validation(
            code: "SmsNotificationData.ContentTooLong",
            description: "SMS content exceeds 160 characters.");
    }
}
