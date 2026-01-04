namespace ReSys.Core.Common.Storage;

public sealed record FileUploadResult(
    string FileId,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    string Hash,
    string Subdirectory,
    DateTime UploadedAt,
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
    DateTime CreatedAt,
    string Extension,
    bool IsEncrypted = false,
    DateTime? ModifiedAt = null,
    Dictionary<string, string>? CustomMetadata = null);

public sealed record FileUploadOptions(
    string? Subdirectory = null,
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
}