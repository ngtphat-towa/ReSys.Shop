namespace ReSys.Core.Common.Constants;

public static class LogTemplates
{
    // 1. Bootstrapping & Lifecycle
    public static class Bootstrapper
    {
        public const string Starting = "[STARTUP] Application starting: {ApplicationName} ({Environment})";
        public const string Stopped = "[STOPPED] Application stopped cleanly";
        public const string StoppedUnexpectedly = "[CRASH] Application stopped unexpectedly";

        public const string Configuring = "[CONFIG] Configuring {Service} with options: {@Options}";
        public const string ModuleRegistering = "[MODULE] [{Layer}] Registering module: {Module}";
        public const string ModuleRegistered = "[MODULE] [{Layer}] Registered module: {Module} ({Elapsed:0.00}ms)";
        public const string ModuleFailed = "[MODULE] [{Layer}] Failed to register module: {Module} after {Elapsed:0.00}ms";

        public const string FeatureDisabled = "[FEATURE] Feature disabled: {Feature}";
        public const string ServiceRegistered = "[DI] Service registered: {Service} ({Lifetime})";
    }

    // 2. Security & Compliance
    public static class Security
    {
        public const string UnauthorizedAccess = "[AUTH] Unauthorized access: {Resource} by {User}";
        public const string PermissionDenied = "[AUTH] Permission denied: {Permission} for user {User} on {Resource}";
        public const string LoginSuccess = "[AUTH] Login successful: {User} from {IpAddress}";
        public const string LoginFailed = "[AUTH] Login failed: {User} from {IpAddress} (Reason: {Reason})";
        public const string TokenRefreshed = "[AUTH] Token refreshed: {User}";
    }

    // 3. External Integrations
    public static class External
    {
        public const string CallStarting = "[EXTERNAL] Call starting: {System} [{Operation}] -> {Endpoint}";
        public const string CallSuccess = "[EXTERNAL] Call success: {System} ({Elapsed:0.00}ms) [Status: {StatusCode}]";
        public const string CallFailed = "[EXTERNAL] Call failed: {System} ({Elapsed:0.00}ms) [Status: {StatusCode}] Error: {Error}";
        public const string CallRetrying = "[EXTERNAL] Call retrying: {System} (Attempt {RetryCount})";

        public const string UnknownProvider = "[EXTERNAL] Unknown provider: {Feature} ({Provider}) - using default";
    }

    // 4. Domain & Business
    public static class Domain
    {
        public const string EntityCreated = "[DOMAIN] Entity created: {Entity} (ID: {Id}) by {User}";
        public const string EntityUpdated = "[DOMAIN] Entity updated: {Entity} (ID: {Id}) Changes: {@Changes}";
        public const string EntityDeleted = "[DOMAIN] Entity deleted: {Entity} (ID: {Id}) by {User}";
        public const string OperationFailed = "[DOMAIN] Operation failed: {Operation} (Reason: {Reason})";
    }

    // 5. Use Cases (CQRS)
    public static class UseCase
    {
        public const string Starting = "[USECASE] Starting: {RequestName}";
        public const string Completed = "[USECASE] Completed: {RequestName} in {Elapsed}ms";
        public const string Failed = "[USECASE] Failed: {RequestName} after {Elapsed}ms";
    }

    // 6. Background Jobs
    public static class Jobs
    {
        public const string JobStarting = "[JOB] Job starting: {JobName} (ID: {JobId})";
        public const string JobCompleted = "[JOB] Job completed: {JobName} ({Elapsed:0.00}ms)";
        public const string JobFailed = "[JOB] Job failed: {JobName} ({Elapsed:0.00}ms)";
    }
}
