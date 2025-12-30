namespace ReSys.Core.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Stream GetFileStream(string fileName);
}
