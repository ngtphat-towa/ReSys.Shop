using ErrorOr;

namespace ReSys.Core.Domain.Settings;

public static class SettingErrors
{
    public static Error NotFound(string key) => Error.NotFound(
        code: "Setting.NotFound",
        description: $"Setting with key '{key}' was not found.");

    public static Error KeyRequired => Error.Validation(
        code: "Setting.KeyRequired",
        description: "Setting key is required.");

    public static Error ValueRequired => Error.Validation(
        code: "Setting.ValueRequired",
        description: "Setting value is required.");
}
