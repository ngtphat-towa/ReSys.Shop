using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Telemetry;
using ReSys.Core.Common.Notifications.Validators;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Infrastructure.Notifications.Services;

using Serilog;

using Sinch;
using Sinch.SMS;

using System.Net;
using System.Net.Mail;

namespace ReSys.Infrastructure.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Infrastructure", "Notifications", s =>
        {
            // Validators - Register manually to avoid NotificationRecipientValidator DI issues
            s.AddScoped<IValidator<NotificationMessage>, NotificationMessageValidator>();
            s.AddScoped<IValidator<NotificationAttachment>, NotificationAttachmentValidator>();

            // Options
            s.AddOptions<SmtpOptions>()
                .Bind(configuration.GetSection(SmtpOptions.Section))
                .ValidateOnStart();

            s.AddOptions<SmsOptions>()
                .Bind(configuration.GetSection(SmsOptions.Section))
                .ValidateOnStart();

            // Services Facade
            s.AddScoped<INotificationService, NotificationService>();

            // Load config for provider selection
            var smtpOptions = configuration.GetSection(SmtpOptions.Section).Get<SmtpOptions>() ?? new SmtpOptions();
            var smsOptions = configuration.GetSection(SmsOptions.Section).Get<SmsOptions>() ?? new SmsOptions();

            AddEmailNotification(s, smtpOptions);
            AddSmsNotification(s, smsOptions);
        });

        return services;
    }

    private static void AddEmailNotification(IServiceCollection services, SmtpOptions options)
    {
        if (!options.EnableEmailNotifications)
        {
            Log.Warning(LogTemplates.Bootstrapper.FeatureDisabled, "Email Notifications");
            services.AddSingleton<IEmailSenderService, EmptyEmailSenderService>();
            return;
        }

        var provider = options.Provider?.ToLowerInvariant();
        switch (provider)
        {
            case "smtp":
                ConfigureSmtp(services, options);
                break;
            case "sendgrid":
                ConfigureSendGrid(services, options);
                break;
            default:
                Log.Warning(LogTemplates.External.UnknownProvider, "Email", options.Provider);
                services.AddSingleton<IEmailSenderService, EmptyEmailSenderService>();
                return;
        }

        services.AddScoped<IEmailSenderService, EmailSenderService>();
        Log.Debug(LogTemplates.Bootstrapper.ServiceRegistered, nameof(IEmailSenderService), "Scoped");
    }

    private static void ConfigureSmtp(IServiceCollection services, SmtpOptions options)
    {
        if (options.SmtpConfig == null)
        {
            Log.Warning("SMTP configuration is missing. Falling back to EmptyEmailSenderService.");
            services.AddSingleton<IEmailSenderService, EmptyEmailSenderService>();
            return;
        }
        var cfg = options.SmtpConfig;

        var smtpClient = new SmtpClient(cfg.Host, cfg.Port)
        {
            EnableSsl = cfg.EnableSsl,
            UseDefaultCredentials = cfg.UseDefaultCredentials
        };

        if (!cfg.UseDefaultCredentials)
        {
            smtpClient.Credentials = new NetworkCredential(cfg.Username, cfg.Password);
        }

        services.AddFluentEmail(options.FromEmail, options.FromName)
                .AddSmtpSender(smtpClient);

        Log.Information(LogTemplates.External.CallStarting, "SMTP", "CONNECT", $"{cfg.Host}:{cfg.Port}");
    }

    private static void ConfigureSendGrid(IServiceCollection services, SmtpOptions options)
    {
        if (options.SendGridConfig == null || string.IsNullOrEmpty(options.SendGridConfig.ApiKey))
        {
            Log.Warning("SendGrid configuration is missing. Falling back to EmptyEmailSenderService.");
            services.AddSingleton<IEmailSenderService, EmptyEmailSenderService>();
            return;
        }

        services.AddFluentEmail(options.FromEmail, options.FromName)
                .AddSendGridSender(options.SendGridConfig.ApiKey);

        Log.Information(LogTemplates.External.CallStarting, "SendGrid", "API", "sendgrid.com");
    }

    private static void AddSmsNotification(IServiceCollection services, SmsOptions options)
    {
        if (!options.EnableSmsNotifications)
        {
            Log.Warning(LogTemplates.Bootstrapper.FeatureDisabled, "SMS Notifications");
            services.AddSingleton<ISmsSenderService, EmptySmsSenderService>();
            return;
        }

        if (options.Provider?.ToLowerInvariant() == "sinch")
        {
            ConfigureSinch(services, options);
            services.AddScoped<ISmsSenderService, SmsSenderService>();
            Log.Debug(LogTemplates.Bootstrapper.ServiceRegistered, nameof(ISmsSenderService), "Scoped");
        }
        else
        {
            Log.Warning(LogTemplates.External.UnknownProvider, "SMS", options.Provider);
            services.AddSingleton<ISmsSenderService, EmptySmsSenderService>();
        }
    }

    private static void ConfigureSinch(IServiceCollection services, SmsOptions options)
    {
        if (options.SinchConfig == null)
        {
            Log.Warning("Sinch configuration is missing. Falling back to EmptySmsSenderService.");
            services.AddSingleton<ISmsSenderService, EmptySmsSenderService>();
            return;
        }
        var cfg = options.SinchConfig;

        services.AddSingleton<ISinchClient>(sp =>
        {
            Log.Information(LogTemplates.External.CallStarting, "Sinch", "INIT", "SinchClient");
            return new SinchClient(
                cfg.ProjectId,
                cfg.KeyId,
                cfg.KeySecret,
                o =>
                {
                    if (!string.IsNullOrWhiteSpace(cfg.SmsRegion))
                    {
                        o.SmsRegion = cfg.SmsRegion.Trim().ToLower() switch
                        {
                            "us" => SmsRegion.Us,
                            "eu" => SmsRegion.Eu,
                            _ => o.SmsRegion
                        };
                    }
                });
        });
    }
}