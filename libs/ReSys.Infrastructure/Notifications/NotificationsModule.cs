using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Common.Notifications.Validators;
using ReSys.Core.Common.Telemetry;
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
        services.RegisterModule("Infrastructure", "Notifications");

        // Options
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.Section))
            .ValidateOnStart();

        services.AddOptions<SmsOptions>()
            .Bind(configuration.GetSection(SmsOptions.Section))
            .ValidateOnStart();

        // Validators
        services.AddScoped<IValidator<NotificationData>, NotificationDataValidator>();
        services.AddScoped<IValidator<EmailNotificationData>, EmailNotificationDataValidator>();
        services.AddScoped<IValidator<SmsNotificationData>, SmsNotificationDataValidator>();

        // Services Facade
        services.AddScoped<INotificationService, NotificationService>();

        // Load config for provider selection
        var smtpOptions = configuration.GetSection(SmtpOptions.Section).Get<SmtpOptions>() ?? new SmtpOptions();
        var smsOptions = configuration.GetSection(SmsOptions.Section).Get<SmsOptions>() ?? new SmsOptions();

        AddEmailNotification(services, smtpOptions);
        AddSmsNotification(services, smsOptions);

        return services;
    }

    private static void AddEmailNotification(IServiceCollection services, SmtpOptions options)
    {
        if (!options.EnableEmailNotifications)
        {
            Log.Warning(LogTemplates.FeatureDisabled, "Email Notifications");
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
                Log.Warning(LogTemplates.UnknownProvider, "Email", options.Provider);
                services.AddSingleton<IEmailSenderService, EmptyEmailSenderService>();
                return;
        }

        services.AddScoped<IEmailSenderService, EmailSenderService>();
        Log.Debug(LogTemplates.ServiceRegistered, nameof(IEmailSenderService), "Scoped");
    }

    private static void ConfigureSmtp(IServiceCollection services, SmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.SmtpConfig);
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

        Log.Information(LogTemplates.ExternalCallStarted, "SMTP", "CONNECT", $"{cfg.Host}:{cfg.Port}");
    }

    private static void ConfigureSendGrid(IServiceCollection services, SmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.SendGridConfig);

        services.AddFluentEmail(options.FromEmail, options.FromName)
                .AddSendGridSender(options.SendGridConfig.ApiKey);

        Log.Information(LogTemplates.ExternalCallStarted, "SendGrid", "API", "sendgrid.com");
    }

    private static void AddSmsNotification(IServiceCollection services, SmsOptions options)
    {
        if (!options.EnableSmsNotifications)
        {
            Log.Warning(LogTemplates.FeatureDisabled, "SMS Notifications");
            services.AddSingleton<ISmsSenderService, EmptySmsSenderService>();
            return;
        }

        if (options.Provider?.ToLowerInvariant() == "sinch")
        {
            ConfigureSinch(services, options);
            services.AddScoped<ISmsSenderService, SmsSenderService>();
            Log.Debug(LogTemplates.ServiceRegistered, nameof(ISmsSenderService), "Scoped");
        }
        else
        {
            Log.Warning(LogTemplates.UnknownProvider, "SMS", options.Provider);
            services.AddSingleton<ISmsSenderService, EmptySmsSenderService>();
        }
    }

    private static void ConfigureSinch(IServiceCollection services, SmsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.SinchConfig);
        var cfg = options.SinchConfig;

        services.AddSingleton<ISinchClient>(sp =>
        {
            Log.Information(LogTemplates.ExternalCallStarted, "Sinch", "INIT", "SinchClient");
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