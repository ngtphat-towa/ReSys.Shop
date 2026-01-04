namespace ReSys.Core.Common.Imaging;

public interface IImageService
{
    /// <summary>
    /// Processes an image stream: resizes if needed, converts to web-friendly format (WebP/JPEG),
    /// and extracts metadata.
    /// </summary>
    /// <param name="originalStream">The input image stream.</param>
    /// <param name="fileName">Original filename.</param>
    /// <param name="maxWidth">Optional max width for resizing.</param>
    /// <param name="maxHeight">Optional max height for resizing.</param>
    /// <returns>A tuple containing the processed stream (which must be disposed) and metadata.</returns>
    Task<(Stream ProcessedStream, ImageMetadata Metadata)> ProcessImageAsync(
        Stream originalStream, 
        string fileName, 
        int? maxWidth = null, 
        int? maxHeight = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts metadata without modifying the stream.
    /// </summary>
    Task<ImageMetadata> GetMetadataAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
