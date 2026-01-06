using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Notifications.Options;

public sealed class SmtpOptions : IValidateOptions<SmtpOptions>
{
    public const string Section = "Notifications:SmtpOptions";

    public bool EnableEmailNotifications { get; set; }
    public string Provider { get; set; } = "logger"; // logger | smtp | sendgrid
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public int? MaxAttachmentSize { get; set; } = 25;

    public SmtpConfig? SmtpConfig { get; set; }
    public SendGridConfig? SendGridConfig { get; set; }

    public ValidateOptionsResult Validate(string? name, SmtpOptions options)
    {
        List<string> errors = [];

        // Validate that EnableEmailNotifications is true if FromEmail and FromName are set
        if (options.EnableEmailNotifications)
        {
            if (string.IsNullOrEmpty(value: options.FromEmail))
            {
                errors.Add(item: "FromEmail must be provided when email notifications are enabled.");
            }

            if (string.IsNullOrEmpty(value: options.FromName))
            {
                errors.Add(item: "FromName must be provided when email notifications are enabled.");
            }
        }

        // Validate provider
        if (string.IsNullOrEmpty(value: options.Provider))
        {
            errors.Add(item: "Provider is required.");
        }
        else if (options.Provider != "logger" && options.Provider != "smtp" && options.Provider != "sendgrid")
        {
            errors.Add(item: $"Invalid provider: {options.Provider}. Allowed values are 'logger', 'smtp', 'sendgrid'.");
        }

        // Validate SmtpConfig when the provider is 'smtp'
        if (options.Provider == "smtp" && options.SmtpConfig != null)
        {
            if (string.IsNullOrEmpty(value: options.SmtpConfig.Host))
            {
                errors.Add(item: "SmtpConfig Host is required for 'smtp' provider.");
            }

            if (options.SmtpConfig.Port <= 0)
            {
                errors.Add(item: "SmtpConfig Port must be a positive integer.");
            }
        }

        // Validate SendGridConfig when the provider is 'sendgrid'
        if (options.Provider == "sendgrid" && options.SendGridConfig != null)
        {
            if (string.IsNullOrEmpty(value: options.SendGridConfig.ApiKey))
            {
                errors.Add(item: "SendGridConfig ApiKey is required for 'sendgrid' provider.");
            }
        }

        // Return validation result
        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(failures: errors);
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class SmtpConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; }
    public bool UseDefaultCredentials { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class SendGridConfig
{
    public string ApiKey { get; set; } = null!;
}
