using ErrorOr;

namespace ReSys.Core.Feature.Common.Notification.Models.Notification;

public partial class NotificationData
{
    public static class Errors
    {
        public static Error MissingUseCase => Error.Validation(
            code: "NotificationData.MissingUseCase",
            description: "Notification use case is required.");

        public static Error MissingReceiver => Error.Validation(
            code: "NotificationData.MissingReceiver",
            description: "At least one receiver is required.");

        public static Error EmptyCreatedBy => Error.Validation(
            code: "NotificationData.EmptyCreatedBy",
            description: "CreatedBy field cannot be empty.");

        public static Error MissingEmailTitle => Error.Validation(
            code: "NotificationData.MissingEmailTitle",
            description: "Email title is required.");

        public static Error MissingEmailContent => Error.Validation(
            code: "NotificationData.MissingEmailContent",
            description: "Email content is required.");

        public static Error InvalidEmailSender => Error.Validation(
            code: "NotificationData.InvalidEmailSender",
            description: "Invalid email sender format.");

        public static Error MissingSmsContent => Error.Validation(
            code: "NotificationData.MissingSmsContent",
            description: "SMS content is required.");

        public static Error SmsContentTooLong => Error.Validation(
            code: "NotificationData.SmsContentTooLong",
            description: "SMS content exceeds 160 characters.");

        public static Error InvalidSmsSender => Error.Validation(
            code: "NotificationData.InvalidSmsSender",
            description: "Invalid SMS sender number format.");

        public static Error NullParameters => Error.Validation(
            code: "NotificationData.NullParameters",
            description: "Parameters cannot be null.");

        public static Error InvalidReceivers => Error.Validation(
            code: "NotificationData.InvalidReceivers",
            description: "Receivers list is invalid.");

        public static Error InvalidTitle => Error.Validation(
            code: "NotificationData.InvalidTitle",
            description: "Title is invalid.");

        public static Error InvalidContent => Error.Validation(
            code: "NotificationData.InvalidContent",
            description: "Content is invalid.");

        public static Error InvalidHtmlContent => Error.Validation(
            code: "NotificationData.InvalidHtmlContent",
            description: "HTML content is invalid.");

        public static Error InvalidCreatedBy => Error.Validation(
            code: "NotificationData.InvalidCreatedBy",
            description: "CreatedBy is invalid.");

        public static Error InvalidLanguage => Error.Validation(
            code: "NotificationData.InvalidLanguage",
            description: "Language is invalid.");
    }
}
