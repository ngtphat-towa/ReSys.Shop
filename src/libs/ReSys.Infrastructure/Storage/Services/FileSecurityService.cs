using System.Security.Cryptography;
using System.Text;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage.Options;

namespace ReSys.Infrastructure.Storage.Services;

public sealed class FileSecurityService(IOptions<StorageOptions> options, ILogger<FileSecurityService> logger) : IFileSecurityService
{
    private readonly StorageOptions _options = options.Value;

    public async Task<ErrorOr<string>> EncryptFileAsync(
        Stream inputStream,
        Stream outputStream,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.Security.EncryptionKey))
            return FileErrors.EncryptionNotConfigured;

        try
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(_options.Security.EncryptionKey);
            aes.GenerateIV();

            await outputStream.WriteAsync(aes.IV, cancellationToken);

            await using var cryptoStream = new CryptoStream(
                outputStream,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write,
                leaveOpen: true);

            await inputStream.CopyToAsync(cryptoStream, _options.BufferSize, cancellationToken);
            await cryptoStream.FlushFinalBlockAsync(cancellationToken);

            return Convert.ToBase64String(aes.IV);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Encryption failed");
            return Error.Failure("File.EncryptionFailed", ex.Message);
        }
    }

    public async Task<ErrorOr<Success>> DecryptFileAsync(
        Stream inputStream,
        Stream outputStream,
        string encryptionKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.Security.EncryptionKey))
            return FileErrors.EncryptionNotConfigured;

        try
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(_options.Security.EncryptionKey);

            var iv = new byte[aes.IV.Length];
            await inputStream.ReadExactlyAsync(iv, cancellationToken);
            aes.IV = iv;

            await using var cryptoStream = new CryptoStream(
                inputStream,
                aes.CreateDecryptor(),
                CryptoStreamMode.Read,
                leaveOpen: true);

            await cryptoStream.CopyToAsync(outputStream, _options.BufferSize, cancellationToken);

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Decryption failed");
            return Error.Failure("File.DecryptionFailed", ex.Message);
        }
    }

    public async Task<ErrorOr<bool>> ScanForMalwareAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (!fileStream.CanSeek)
            return true;

        var originalPosition = fileStream.Position;
        try
        {
            fileStream.Position = 0;
            var buffer = new byte[4096];
            var bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

            var eicar = Encoding.ASCII.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}");
            if (ContainsPattern(buffer, bytesRead, eicar))
            {
                logger.LogWarning("EICAR test pattern detected");
                return false;
            }

            return true;
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }

    private static byte[] DeriveKey(string password)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool ContainsPattern(byte[] buffer, int length, byte[] pattern)
    {
        for (int i = 0; i <= length - pattern.Length; i++)
        {
            if (buffer.AsSpan(i, pattern.Length).SequenceEqual(pattern))
                return true;
        }
        return false;
    }
}