using ErrorOr;

using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Models;

namespace ReSys.Core.Common.Notifications.Builders;

public static class NotificationMessageBuilder
{
    #region Message Builder Initializers
    /// <summary>
    /// Starts the builder flow by specifying the UseCase first.
    /// </summary>
    public static UseCaseBuilder ForUseCase(NotificationConstants.UseCase useCase)
    {
        return new UseCaseBuilder(useCase);
    }

    /// <summary>
    /// Legacy/Direct initializer.
    /// </summary>
    public static ErrorOr<NotificationMessage> Create(
        NotificationConstants.UseCase useCase,
        NotificationRecipient recipient)
    {
        return new NotificationMessage(useCase, recipient, NotificationContext.Empty);
    }
    #endregion

    #region Message Chaining
    public static ErrorOr<NotificationMessage> WithMetadata(
        this ErrorOr<NotificationMessage> result,
        NotificationMetadata metadata)
    {
        if (result.IsError) return result.Errors;
        return result.Value with { Metadata = metadata };
    }

    public static ErrorOr<NotificationMessage> AddAttachment(
        this ErrorOr<NotificationMessage> result,
        NotificationAttachment attachment)
    {
        if (result.IsError) return result.Errors;
        var attachments = result.Value.Attachments != null
            ? new List<NotificationAttachment>(result.Value.Attachments)
            : new List<NotificationAttachment>();

        attachments.Add(attachment);
        return result.Value with { Attachments = attachments };
    }

    public static ErrorOr<NotificationMessage> WithContext(
        this ErrorOr<NotificationMessage> result,
        NotificationContext context)
    {
        if (result.IsError) return result.Errors;
        return result.Value with { Context = context };
    }

    public static ErrorOr<NotificationMessage> AddParam(
        this ErrorOr<NotificationMessage> result,
        NotificationConstants.Parameter parameter,
        string? value)
    {
        if (result.IsError) return result.Errors;
        var newContext = NotificationContext.ApplyParameter(result.Value.Context, parameter, value);
        return result.Value with { Context = newContext };
    }
    #endregion

    #region Context Builder
    public static ErrorOr<NotificationContext> CreateContext() => NotificationContext.Empty;

    public static ErrorOr<NotificationContext> AddParam(this ErrorOr<NotificationContext> result, NotificationConstants.Parameter parameter, string? value)
    {
        if (result.IsError) return result.Errors;
        return NotificationContext.ApplyParameter(result.Value, parameter, value);
    }

    public static ErrorOr<NotificationContext> AddParams(this ErrorOr<NotificationContext> result, IDictionary<NotificationConstants.Parameter, string?> parameters)
    {
        if (result.IsError) return result.Errors;
        var context = result.Value;
        foreach (var param in parameters)
        {
            context = NotificationContext.ApplyParameter(context, param.Key, param.Value);
        }
        return context;
    }
    #endregion

    #region Semantic Helpers (Message)
    public static ErrorOr<NotificationMessage> WithUserFirstName(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.UserFirstName, value);

    public static ErrorOr<NotificationMessage> WithUserEmail(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.UserEmail, value);

    public static ErrorOr<NotificationMessage> WithOrderId(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OrderId, value);

    public static ErrorOr<NotificationMessage> WithSystemName(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.SystemName, value);

    public static ErrorOr<NotificationMessage> WithOtpCode(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OtpCode, value);

    public static ErrorOr<NotificationMessage> WithPromoCode(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.PromoCode, value);

    public static ErrorOr<NotificationMessage> WithOrderTotal(this ErrorOr<NotificationMessage> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OrderTotal, value);
    #endregion

    #region Semantic Helpers (Context)
    public static ErrorOr<NotificationContext> ContextWithUserFirstName(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.UserFirstName, value);

    public static ErrorOr<NotificationContext> ContextWithUserEmail(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.UserEmail, value);

    public static ErrorOr<NotificationContext> ContextWithOrderId(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OrderId, value);

    public static ErrorOr<NotificationContext> ContextWithSystemName(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.SystemName, value);

    public static ErrorOr<NotificationContext> ContextWithOtpCode(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OtpCode, value);

    public static ErrorOr<NotificationContext> ContextWithPromoCode(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.PromoCode, value);

    public static ErrorOr<NotificationContext> ContextWithOrderTotal(this ErrorOr<NotificationContext> result, string value)
        => result.AddParam(NotificationConstants.Parameter.OrderTotal, value);
    #endregion

    /// <summary>
    /// Intermediate builder to support split initialization.
    /// </summary>
    public sealed class UseCaseBuilder(NotificationConstants.UseCase useCase)
    {
        public ErrorOr<NotificationMessage> To(NotificationRecipient recipient)
        {
            return new NotificationMessage(useCase, recipient, NotificationContext.Empty);
        }
    }
}
