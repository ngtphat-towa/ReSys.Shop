namespace ReSys.Core.Common.Constants;

public static class LogTemplates
{
    public const string FeatureDisabled = "{Feature} is disabled.";
    public const string UnknownProvider = "Unknown provider for {Feature}: {Provider}.";
    public const string ServiceRegistered = "Service {Service} registered with lifetime {Lifetime}.";
    public const string ExternalCallStarted = "Starting external call to {System} ({Action}) at {Target}.";
    public const string ModuleRegistered = "Module {Module} registered with {Count} components.";
}
