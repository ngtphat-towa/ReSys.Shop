using ErrorOr;

namespace ReSys.Core.Common.Storage;

public static class FileErrors
{
    public static Error NotFound(string fileId) => Error.NotFound(
        "File.NotFound",
        $"File with ID '{fileId}' was not found");

    public static Error InvalidFileName => Error.Validation(
        "File.InvalidFileName",
        "File name is empty or contains invalid characters");

    public static Error InvalidExtension(string extension, string[] allowed) => Error.Validation(
        "File.InvalidExtension",
        $"Extension '{extension}' is not allowed. Allowed: {string.Join(", ", allowed)}");

    public static Error DangerousExtension(string extension) => Error.Validation(
        "File.DangerousExtension",
        $"Extension '{extension}' is blocked for security reasons");

    public static Error FileTooLarge(long size, long maxSize) => Error.Validation(
        "File.TooLarge",
        $"File size ({FormatBytes(size)}) exceeds maximum ({FormatBytes(maxSize)})");

    public static Error EmptyFile => Error.Validation(
        "File.Empty",
        "File is empty");

    public static Error SignatureMismatch => Error.Validation(
        "File.SignatureMismatch",
        "File content does not match its extension");

    public static Error MalwareDetected => Error.Validation(
        "File.MalwareDetected",
        "File failed malware scan");

    public static Error AlreadyExists(string fileId) => Error.Conflict(
        "File.AlreadyExists",
        $"File '{fileId}' already exists");

    public static Error EncryptionNotConfigured => Error.Failure(
        "File.EncryptionNotConfigured",
        "Encryption is not configured");

    public static Error SaveFailed(string reason) => Error.Failure(
        "File.SaveFailed",
        $"Failed to save file: {reason}");

    public static Error DeleteFailed(string reason) => Error.Failure(
        "File.DeleteFailed",
        $"Failed to delete file: {reason}");

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}