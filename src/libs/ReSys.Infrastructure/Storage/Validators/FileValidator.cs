using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage.Options;

namespace ReSys.Infrastructure.Storage.Validators;

public sealed class FileValidator(IOptions<StorageOptions> options, ILogger<FileValidator> logger) : IFileValidator
{
    private readonly StorageOptions _options = options.Value;
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
        { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }
    };

    public async Task<ErrorOr<Success>> ValidateAsync(
        Stream fileStream,
        string fileName,
        FileUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return FileErrors.InvalidFileName;

        if (ContainsDangerousCharacters(fileName))
            return FileErrors.InvalidFileName;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
            return FileErrors.InvalidFileName;

        if (_options.Security.DangerousExtensions.Contains(extension))
            return FileErrors.DangerousExtension(extension);

        var allowedExtensions = options.AllowedExtensions ?? _options.AllowedExtensions;
        if (allowedExtensions.Length > 0 && !allowedExtensions.Contains(extension))
            return FileErrors.InvalidExtension(extension, allowedExtensions);

        if (fileStream.CanSeek)
        {
            var maxSize = options.MaxFileSize ?? _options.MaxFileSize;
            if (fileStream.Length > maxSize)
                return FileErrors.FileTooLarge(fileStream.Length, maxSize);

            if (fileStream.Length == 0)
                return FileErrors.EmptyFile;

            if (options.ValidateContent && _options.Security.ValidateFileSignatures)
            {
                var signatureValid = await ValidateFileSignatureAsync(fileStream, extension, cancellationToken);
                if (!signatureValid)
                {
                    logger.LogWarning("File signature mismatch for {FileName}. Content does not match {Extension}.", fileName, extension);
                    return FileErrors.SignatureMismatch;
                }
            }
        }

        return Result.Success;
    }

    private async Task<bool> ValidateFileSignatureAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken)
    {
        if (!FileSignatures.TryGetValue(extension, out var signatures))
            return true;

        if (!stream.CanSeek)
            return true;

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            var maxLen = signatures.Max(s => s.Length);
            var buffer = new byte[maxLen];
            
            // Read exactly what we need
            int totalRead = 0;
            while (totalRead < maxLen)
            {
                int read = await stream.ReadAsync(buffer, totalRead, maxLen - totalRead, cancellationToken);
                if (read == 0) break;
                totalRead += read;
            }

            if (totalRead == 0) return false;

            foreach (var signature in signatures)
            {
                if (totalRead >= signature.Length)
                {
                    if (buffer.AsSpan(0, signature.Length).SequenceEqual(signature))
                        return true;
                }
            }

            logger.LogWarning("Signature mismatch for {Extension}. Expected one of {Expected}. Actual: {Actual}", 
                extension, 
                string.Join(" | ", signatures.Select(s => BitConverter.ToString(s))),
                BitConverter.ToString(buffer, 0, totalRead));

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating file signature");
            return true; 
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool ContainsDangerousCharacters(string fileName)
    {
        var dangerous = new[] { "..", "~", "<", ">", ":", "\"", "|", "?", "*", "\0" };
        return dangerous.Any(fileName.Contains) || fileName.Any(c => c < 32);
    }
}
