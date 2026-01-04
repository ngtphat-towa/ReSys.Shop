using ReSys.Core.Common.Imaging;


using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ReSys.Infrastructure.Imaging;

public class ImageService : IImageService
{
    public async Task<(Stream ProcessedStream, ImageMetadata Metadata)> ProcessImageAsync(
        Stream originalStream,
        string fileName,
        int? maxWidth = null,
        int? maxHeight = null,
        CancellationToken cancellationToken = default)
    {
        // Reset stream position if possible
        if (originalStream.CanSeek)
        {
            originalStream.Position = 0;
        }

        using var image = await Image.LoadAsync(originalStream, cancellationToken);
        
        // Resize logic
        if (maxWidth.HasValue || maxHeight.HasValue)
        {
            var options = new ResizeOptions
            {
                Size = new Size(maxWidth ?? 0, maxHeight ?? 0),
                Mode = ResizeMode.Max
            };
            image.Mutate(x => x.Resize(options));
        }

        var outStream = new MemoryStream();
        // Convert to WebP for optimization
        await image.SaveAsWebpAsync(outStream, cancellationToken);
        outStream.Position = 0;

        var metadata = new ImageMetadata(
            image.Width,
            image.Height,
            "webp",
            outStream.Length,
            Path.ChangeExtension(fileName, ".webp")
        );

        return (outStream, metadata);
    }

    public async Task<ImageMetadata> GetMetadataAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek) stream.Position = 0;

        var info = await Image.IdentifyAsync(stream, cancellationToken);
        
        return new ImageMetadata(
            info.Width,
            info.Height,
            info.Metadata.DecodedImageFormat?.Name?.ToLower() ?? "unknown",
            stream.Length,
            fileName
        );
    }
}
