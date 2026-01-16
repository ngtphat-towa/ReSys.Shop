namespace ReSys.Core.Common.Storage;

public sealed record FileUploadResult(
    string FileId,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    string Hash,
    string Subdirectory,
    DateTimeOffset UploadedAt,
    string? EncryptionKey = null,
    Dictionary<string, string>? Metadata = null);

public sealed record FileMetadata(
    string FileId,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    string Hash,
    string Subdirectory,
    DateTimeOffset CreatedAt,
    string Extension,
    bool IsEncrypted = false,
    DateTimeOffset? ModifiedAt = null,
    Dictionary<string, string>? CustomMetadata = null);

public sealed record FileUploadOptions(
    string[]? Subdirectories = null,
    long? MaxFileSize = null,
    string[]? AllowedExtensions = null,
    bool ValidateContent = true,
    bool ScanForMalware = false,
    bool EncryptFile = false,
    bool GenerateHash = true,
    bool PreserveOriginalName = false,
    bool OverwriteExisting = false,
    Dictionary<string, string>? CustomMetadata = null)
{
    public static FileUploadOptions Default => new();

    /// <summary>
    /// Convenience constructor for a single subdirectory.
    /// </summary>
    public FileUploadOptions(string subdirectory) : this(new[] { subdirectory })
    {
    }

    /// <summary>
    /// Gets the safely combined path string.
    /// </summary>
    public string? GetPath() =>
        Subdirectories?.Length > 0 ? string.Join("/", Subdirectories) : null;
}
