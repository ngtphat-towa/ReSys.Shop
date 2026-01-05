namespace ReSys.Core.Common.Notification.Constants;

public static class LogTemplates
{
    public const string FeatureDisabled = "{Feature} is disabled.";
    public const string UnknownProvider = "Unknown {Type} provider: {Provider}";
    public const string ServiceRegistered = "{Service} registered as {Lifetime}.";
    public const string ExternalCallStarted = "External call started: {System} {Action} {Details}";
    public const string ModuleRegistered = "Module {Module} registered.";
}
