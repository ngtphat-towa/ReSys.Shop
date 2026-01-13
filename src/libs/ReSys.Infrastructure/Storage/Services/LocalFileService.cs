using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage.Options;

namespace ReSys.Infrastructure.Storage.Services;

public sealed class LocalFileService : IFileService
{
    private readonly string _storagePath;
    private readonly StorageOptions _options;
    private readonly ILogger<LocalFileService> _logger;
    private readonly IFileValidator _validator;
    private readonly IFileSecurityService? _securityService;

    public LocalFileService(
        IOptions<StorageOptions> options,
        ILogger<LocalFileService> logger,
        IFileValidator validator,
        IFileSecurityService? securityService = null)
    {
        _options = options.Value;
        _logger = logger;
        _validator = validator;
        _securityService = securityService;

        _storagePath = Path.IsPathRooted(_options.LocalPath)
            ? _options.LocalPath
            : Path.Combine(Directory.GetCurrentDirectory(), _options.LocalPath);

        EnsureDirectoryStructure();
    }

    public async Task<ErrorOr<FileUploadResult>> SaveFileAsync(
        Stream fileStream,
        string fileName,
        FileUploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= FileUploadOptions.Default;

        var validationResult = await _validator.ValidateAsync(fileStream, fileName, options, cancellationToken);
        if (validationResult.IsError)
            return validationResult.Errors;

        if (options.ScanForMalware && _securityService != null)
        {
            var scanResult = await _securityService.ScanForMalwareAsync(fileStream, cancellationToken);
            if (scanResult.IsError)
                return scanResult.Errors;
            if (!scanResult.Value)
                return FileErrors.MalwareDetected;
        }

        var fileId = options.PreserveOriginalName
            ? SanitizeFileName(fileName)
            : $"{Guid.NewGuid()}_{SanitizeFileName(fileName)}";

        var subdirectory = options.GetPath() ?? "temp";
        var targetDir = Path.Combine(_storagePath, subdirectory);

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        var filePath = Path.Combine(targetDir, fileId);

        if (!options.OverwriteExisting && File.Exists(filePath))
            return FileErrors.AlreadyExists(fileId);

        try
        {
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            string? encryptionKey = null;

            if (options.EncryptFile && _securityService != null)
            {
                await using var tempStream = new MemoryStream();
                await fileStream.CopyToAsync(tempStream, cancellationToken);
                tempStream.Position = 0;

                await using var outputStream = new FileStream(
                    filePath, FileMode.Create, FileAccess.Write,
                    FileShare.None, _options.BufferSize, useAsync: true);

                var encryptResult = await _securityService.EncryptFileAsync(
                    tempStream, outputStream, cancellationToken);

                if (encryptResult.IsError)
                    return encryptResult.Errors;

                encryptionKey = encryptResult.Value;
            }
            else
            {
                await using var outputStream = new FileStream(
                    filePath, FileMode.Create, FileAccess.Write,
                    FileShare.None, _options.BufferSize, useAsync: true);

                await fileStream.CopyToAsync(outputStream, _options.BufferSize, cancellationToken);
            }

            var hash = options.GenerateHash
                ? await CalculateFileHashAsync(filePath, cancellationToken)
                : string.Empty;

            var fileInfo = new FileInfo(filePath);
            var result = new FileUploadResult(
                FileId: fileId,
                FileName: fileId,
                OriginalFileName: fileName,
                FileSize: fileInfo.Length,
                ContentType: GetContentType(fileName),
                Hash: hash,
                Subdirectory: subdirectory,
                UploadedAt: DateTimeOffset.UtcNow,
                EncryptionKey: encryptionKey,
                Metadata: options.CustomMetadata
            );

            await SaveMetadataAsync(fileId, result, cancellationToken);

            _logger.LogInformation("File saved: {FileId}", fileId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);

            if (File.Exists(filePath))
                try { File.Delete(filePath); } catch { }

            return FileErrors.SaveFailed(ex.Message);
        }
    }

    public async Task<ErrorOr<Stream>> GetFileStreamAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var metadataResult = await GetFileMetadataAsync(fileId, cancellationToken);
        if (metadataResult.IsError)
            return metadataResult.Errors;

        var filePath = FindFilePath(fileId);
        if (string.IsNullOrEmpty(filePath))
            return FileErrors.NotFound(fileId);

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (metadataResult.Value.IsEncrypted && _securityService != null)
        {
            var decryptedStream = new MemoryStream();
            var decryptResult = await _securityService.DecryptFileAsync(
                fileStream, decryptedStream, metadataResult.Value.Hash, cancellationToken);

            await fileStream.DisposeAsync();

            if (decryptResult.IsError)
                return decryptResult.Errors;

            decryptedStream.Position = 0;
            return decryptedStream;
        }

        return fileStream;
    }

    public async Task<ErrorOr<Deleted>> DeleteFileAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var filePath = FindFilePath(fileId);
        if (string.IsNullOrEmpty(filePath))
            return FileErrors.NotFound(fileId);

        try
        {
            await DeleteMetadataAsync(fileId, cancellationToken);
            await Task.Run(() => File.Delete(filePath), cancellationToken);

            _logger.LogInformation("File deleted: {FileId}", fileId);
            return Result.Deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete: {FileId}", fileId);
            return FileErrors.DeleteFailed(ex.Message);
        }
    }

    public Task<ErrorOr<bool>> FileExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var filePath = FindFilePath(fileId);
        var exists = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        return Task.FromResult<ErrorOr<bool>>(exists);
    }

    public async Task<ErrorOr<FileMetadata>> GetFileMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var metadataPath = GetMetadataPath(fileId);

        if (!File.Exists(metadataPath))
        {
            var filePath = FindFilePath(fileId);
            if (string.IsNullOrEmpty(filePath))
                return FileErrors.NotFound(fileId);

            var fileInfo = new FileInfo(filePath);
            return new FileMetadata(
                FileId: fileId,
                FileName: fileId,
                OriginalFileName: fileId,
                FileSize: fileInfo.Length,
                ContentType: GetContentType(fileId),
                Hash: string.Empty,
                Subdirectory: "unknown",
                CreatedAt: new DateTimeOffset(fileInfo.CreationTimeUtc),
                Extension: fileInfo.Extension,
                ModifiedAt: new DateTimeOffset(fileInfo.LastWriteTimeUtc)
            );
        }

        var json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
        var metadata = JsonSerializer.Deserialize<FileMetadata>(json);

        if (metadata is null)
            return FileErrors.NotFound(fileId);

        return metadata;
    }

    public async Task<ErrorOr<string>> GetFileHashAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var metadataResult = await GetFileMetadataAsync(fileId, cancellationToken);
        if (metadataResult.IsError)
            return metadataResult.Errors;

        if (!string.IsNullOrEmpty(metadataResult.Value.Hash))
            return metadataResult.Value.Hash;

        var filePath = FindFilePath(fileId);
        if (string.IsNullOrEmpty(filePath))
            return FileErrors.NotFound(fileId);

        return await CalculateFileHashAsync(filePath, cancellationToken);
    }

    public async Task<ErrorOr<List<FileMetadata>>> ListFilesAsync(
        string? subdirectory = null,
        CancellationToken cancellationToken = default)
    {
        var targetDir = string.IsNullOrEmpty(subdirectory)
            ? _storagePath
            : Path.Combine(_storagePath, subdirectory);

        if (!Directory.Exists(targetDir))
            return new List<FileMetadata>();

        var files = Directory.GetFiles(targetDir);
        var metadataList = new List<FileMetadata>();

        foreach (var file in files)
        {
            var fileId = Path.GetFileName(file);
            var metadataResult = await GetFileMetadataAsync(fileId, cancellationToken);
            if (!metadataResult.IsError)
                metadataList.Add(metadataResult.Value);
        }

        return metadataList;
    }

    public async Task<ErrorOr<Success>> MoveFileAsync(
        string fileId,
        string destinationSubdirectory,
        CancellationToken cancellationToken = default)
    {
        var sourcePath = FindFilePath(fileId);
        if (string.IsNullOrEmpty(sourcePath))
            return FileErrors.NotFound(fileId);

        var destDir = Path.Combine(_storagePath, destinationSubdirectory);
        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        var destPath = Path.Combine(destDir, fileId);

        try
        {
            await Task.Run(() => File.Move(sourcePath, destPath, overwrite: false), cancellationToken);

            var metadataResult = await GetFileMetadataAsync(fileId, cancellationToken);
            if (!metadataResult.IsError)
            {
                var updated = metadataResult.Value with
                {
                    Subdirectory = destinationSubdirectory,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await SaveMetadataAsync(fileId, updated, cancellationToken);
            }

            _logger.LogInformation("File moved: {FileId} to {Subdirectory}", fileId, destinationSubdirectory);
            return Result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move: {FileId}", fileId);
            return Error.Failure("File.MoveFailed", ex.Message);
        }
    }

    public async Task<ErrorOr<FileUploadResult>> CopyFileAsync(
        string sourceFileId,
        string? destinationSubdirectory = null,
        CancellationToken cancellationToken = default)
    {
        var sourcePath = FindFilePath(sourceFileId);
        if (string.IsNullOrEmpty(sourcePath))
            return FileErrors.NotFound(sourceFileId);

        var metadataResult = await GetFileMetadataAsync(sourceFileId, cancellationToken);
        if (metadataResult.IsError)
            return metadataResult.Errors;

        await using var sourceStream = File.OpenRead(sourcePath);

        var options = new FileUploadOptions(destinationSubdirectory ?? metadataResult.Value.Subdirectory)
        {
            GenerateHash = true,
            ValidateContent = false
        };

        return await SaveFileAsync(sourceStream, metadataResult.Value.OriginalFileName, options, cancellationToken);
    }

    // Helper methods

    private void EnsureDirectoryStructure()
    {
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);

        var metadataPath = Path.Combine(_storagePath, ".metadata");
        if (!Directory.Exists(metadataPath))
            Directory.CreateDirectory(metadataPath);
    }

    private string? FindFilePath(string fileId)
    {
        // 1. Try as direct relative path (e.g. "examples/nested/guid_name.webp")
        var directPath = Path.Combine(_storagePath, fileId);
        if (File.Exists(directPath)) return directPath;

        // 2. Check all existing subdirectories (recursively) using just the filename
        var fileName = Path.GetFileName(fileId);
        if (Directory.Exists(_storagePath))
        {
            var files = Directory.GetFiles(_storagePath, fileName, SearchOption.AllDirectories);
            // Return first match that isn't in metadata
            var match = files.FirstOrDefault(f => !f.Contains(".metadata"));
            if (match != null) return match;
        }

        return null;
    }

    private string GetMetadataPath(string fileId)
    {
        var fileName = Path.GetFileName(fileId);
        return Path.Combine(_storagePath, ".metadata", $"{SanitizeSegment(fileName)}.json");
    }

    private static string SanitizeFileName(string fileName)
    {
        if (fileName.Contains('/') || fileName.Contains('\\'))
        {
            var segments = fileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Path.DirectorySeparatorChar, segments.Select(SanitizeSegment));
        }

        return SanitizeSegment(fileName);
    }

    private static string SanitizeSegment(string segment)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(segment.Length);
        foreach (var c in segment)
        {
            sanitized.Append(invalidChars.Contains(c) ? '_' : c);
        }
        return sanitized.ToString();
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            ".json" => "application/json",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private async Task SaveMetadataAsync(string fileId, object metadata, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(fileId);
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
    }

    private async Task DeleteMetadataAsync(string fileId, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(fileId);
        if (File.Exists(metadataPath))
            await Task.Run(() => File.Delete(metadataPath), cancellationToken);
    }

    private static async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
