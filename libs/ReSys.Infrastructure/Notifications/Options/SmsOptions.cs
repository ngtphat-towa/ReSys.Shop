using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Notifications.Options;

public sealed class SmsOptions : IValidateOptions<SmsOptions>
{
    public const string Section = "Notifications:SmsOptions";
    public bool EnableSmsNotifications { get; set; }
    public string DefaultSenderNumber { get; set; } = null!;
    public string Provider { get; set; } = "logger"; // logger | sinch

    public SinchConfig? SinchConfig { get; set; }

    public ValidateOptionsResult Validate(string? name, SmsOptions options)
    {
        List<string> errors = [];

        // Validate that EnableSmsNotifications is true if DefaultSenderNumber is set
        if (options.EnableSmsNotifications)
        {
            if (string.IsNullOrEmpty(value: options.DefaultSenderNumber))
            {
                errors.Add(item: "DefaultSenderNumber must be provided when SMS notifications are enabled.");
            }
        }

        if (options.Provider == "sinch" && options.SinchConfig != null)
        {
             // Validate SinchConfig
            if (string.IsNullOrEmpty(value: options.SinchConfig.ProjectId))
            {
                errors.Add(item: "SinchConfig ProjectId is required.");
            }

            if (string.IsNullOrEmpty(value: options.SinchConfig.KeyId))
            {
                errors.Add(item: "SinchConfig KeyId is required.");
            }

            if (string.IsNullOrEmpty(value: options.SinchConfig.KeySecret))
            {
                errors.Add(item: "SinchConfig KeySecret is required.");
            }

            if (string.IsNullOrEmpty(value: options.SinchConfig.SenderPhoneNumber))
            {
                errors.Add(item: "SinchConfig SenderPhoneNumber is required.");
            }

            if (string.IsNullOrEmpty(value: options.SinchConfig.SmsRegion))
            {
                errors.Add(item: "SinchConfig SmsRegion is required.");
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
public sealed class SinchConfig
{
    public string ProjectId { get; set; } = null!;
    public string KeyId { get; set; } = null!;
    public string KeySecret { get; set; } = null!;
    public string SenderPhoneNumber { get; set; } = null!;
    public string SmsRegion { get; set; } = null!; // e.g. "Us", "Eu"
}
