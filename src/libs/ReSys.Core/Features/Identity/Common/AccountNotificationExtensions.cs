using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Common.Options.Systems;
using ReSys.Core.Common.Notifications.Builders;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Common;

public static class AccountNotificationExtensions
{
    private static StorefrontOption GetStorefrontOption(IConfiguration configuration)
    {
        return configuration.GetSection(StorefrontOption.Section).Get<StorefrontOption>() 
               ?? new StorefrontOption();
    }

    public static async Task<ErrorOr<Success>> GenerateAndSendConfirmationEmailAsync(
        this UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration,
        User user,
        string? newEmail = null,
        CancellationToken cancellationToken = default)
    {
        var options = GetStorefrontOption(configuration);
        
        string code = !string.IsNullOrWhiteSpace(newEmail)
            ? await userManager.GenerateChangeEmailTokenAsync(user, newEmail)
            : await userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        string confirmUrl = $"{options.BaseUrl.TrimEnd('/')}/confirm-email?userId={user.Id}&code={code}";
        if (!string.IsNullOrEmpty(newEmail)) confirmUrl += $"&newEmail={newEmail}";

        var email = newEmail ?? user.Email;
        if (string.IsNullOrEmpty(email)) return Error.Validation("Email.Required");

        var messageResult = NotificationMessageBuilder
            .ForUseCase(NotificationConstants.UseCase.SystemActiveEmail)
            .To(NotificationRecipient.Create(email, user.FullName))
            .AddParam(NotificationConstants.Parameter.UserName, user.UserName ?? string.Empty)
            .AddParam(NotificationConstants.Parameter.ActiveUrl, HtmlEncoder.Default.Encode(confirmUrl))
            .AddParam(NotificationConstants.Parameter.SystemName, options.SystemName)
            .AddParam(NotificationConstants.Parameter.SupportEmail, options.SupportEmail);

        if (messageResult.IsError) return messageResult.Errors;

        return await notificationService.SendAsync(messageResult.Value, cancellationToken);
    }

    public static async Task<ErrorOr<Success>> GenerateAndSendConfirmationSmsAsync(
        this UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration,
        User user,
        string? newPhoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        var options = GetStorefrontOption(configuration);

        string code = !string.IsNullOrWhiteSpace(newPhoneNumber)
            ? await userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber)
            : await userManager.GenerateUserTokenAsync(user, "Phone", "phone-confirmation");

        var phoneNumber = newPhoneNumber ?? user.PhoneNumber;
        if (string.IsNullOrEmpty(phoneNumber)) return Error.Validation("PhoneNumber.Required");

        var messageResult = NotificationMessageBuilder
            .ForUseCase(NotificationConstants.UseCase.SystemActivePhone)
            .To(NotificationRecipient.Create(phoneNumber, user.FullName))
            .AddParam(NotificationConstants.Parameter.UserName, user.UserName ?? string.Empty)
            .AddParam(NotificationConstants.Parameter.OtpCode, code)
            .AddParam(NotificationConstants.Parameter.SystemName, options.SystemName);

        if (messageResult.IsError) return messageResult.Errors;

        return await notificationService.SendAsync(messageResult.Value, cancellationToken);
    }

    public static async Task<ErrorOr<Success>> GenerateAndSendPasswordResetCodeAsync(
        this UserManager<User> userManager,
        INotificationService notificationService,
        IConfiguration configuration,
        User user,
        CancellationToken cancellationToken = default)
    {
        var options = GetStorefrontOption(configuration);

        string code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        string resetUrl = $"{options.BaseUrl.TrimEnd('/')}/reset-password?userId={user.Id}&code={code}";

        var email = user.Email;
        if (string.IsNullOrEmpty(email)) return Error.Validation("Email.Required");

        var messageResult = NotificationMessageBuilder
            .ForUseCase(NotificationConstants.UseCase.SystemResetPassword)
            .To(NotificationRecipient.Create(email, user.FullName))
            .AddParam(NotificationConstants.Parameter.UserName, user.UserName ?? string.Empty)
            .AddParam(NotificationConstants.Parameter.ResetPasswordUrl, HtmlEncoder.Default.Encode(resetUrl))
            .AddParam(NotificationConstants.Parameter.SystemName, options.SystemName)
            .AddParam(NotificationConstants.Parameter.SupportEmail, options.SupportEmail);

        if (messageResult.IsError) return messageResult.Errors;

        return await notificationService.SendAsync(messageResult.Value, cancellationToken);
    }
}