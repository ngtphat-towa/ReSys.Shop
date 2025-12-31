using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReSys.Core.Interfaces;
using ReSys.Infrastructure.Options;

namespace ReSys.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalFileService> _logger;

    public LocalFileService(IOptions<StorageOptions> options, ILogger<LocalFileService> logger)
    {
        _logger = logger;
        
        var localPath = options.Value.LocalPath;
        _storagePath = Path.IsPathRooted(localPath) 
            ? localPath 
            : Path.Combine(Directory.GetCurrentDirectory(), localPath);

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        _logger.LogInformation("Saving file to {FilePath}", filePath);

        using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        // Return relative path or URL depending on needs. Here just the filename for simplicity.
        return uniqueFileName;
    }

    public Stream GetFileStream(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", fileName);
        }
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }
}
