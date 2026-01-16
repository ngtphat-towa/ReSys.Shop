using ErrorOr;

namespace ReSys.Core.Common.Storage;

public interface IFileService
{
    Task<ErrorOr<FileUploadResult>> SaveFileAsync(
        Stream fileStream,
        string fileName,
        FileUploadOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Stream>> GetFileStreamAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Deleted>> DeleteFileAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<bool>> FileExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<FileMetadata>> GetFileMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<string>> GetFileHashAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<List<FileMetadata>>> ListFilesAsync(
        string? subdirectory = null,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Success>> MoveFileAsync(
        string fileId,
        string destinationSubdirectory,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<FileUploadResult>> CopyFileAsync(
        string sourceFileId,
        string? destinationSubdirectory = null,
        CancellationToken cancellationToken = default);
}

public interface IFileValidator
{
    Task<ErrorOr<Success>> ValidateAsync(
        Stream fileStream,
        string fileName,
        FileUploadOptions options,
        CancellationToken cancellationToken = default);
}

public interface IFileSecurityService
{
    Task<ErrorOr<string>> EncryptFileAsync(
        Stream inputStream,
        Stream outputStream,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<Success>> DecryptFileAsync(
        Stream inputStream,
        Stream outputStream,
        string encryptionKey,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<bool>> ScanForMalwareAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
