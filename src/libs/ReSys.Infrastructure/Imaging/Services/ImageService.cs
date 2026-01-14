
using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Core.Common.Imaging;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using ReSys.Infrastructure.Imaging.Options;

namespace ReSys.Infrastructure.Imaging.Services;

public sealed class ImageService(ILogger<ImageService> logger, IOptions<ImageOptions>? options = null) : IImageService
{
    private readonly ImageOptions _options = options?.Value ?? ImageOptions.Default;

    public async Task<ErrorOr<ProcessedImageResult>> ProcessAsync(
        Stream stream,
        string fileName,
        int? maxWidth = null,
        int? maxHeight = null,
        bool generateThumbnail = true,
        bool generateResponsive = false,
        CancellationToken ct = default)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;

            using var image = await Image.LoadAsync(stream, ct);

            // Validate size
            if (image.Width > _options.MaxWidth || image.Height > _options.MaxHeight)
                return ImageErrors.TooLarge(image.Width, image.Height, _options.MaxWidth, _options.MaxHeight);

            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Resize main image if needed
            if (maxWidth.HasValue || maxHeight.HasValue)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth ?? 0, maxHeight ?? 0),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            // Save main variant
            var main = await SaveVariantAsync(image, fileName, "main", _options.Quality, ct);
            if (main.IsError) return main.Errors;

            // Generate thumbnail
            ImageVariant? thumbnail = null;
            if (generateThumbnail)
            {
                var thumbResult = await GenerateThumbnailAsync(image, fileName, ct);
                if (!thumbResult.IsError)
                    thumbnail = thumbResult.Value;
            }

            // Generate responsive sizes
            var responsive = new List<ImageVariant>();
            if (generateResponsive)
            {
                var responsiveResult = await GenerateResponsiveAsync(image, fileName, ct);
                if (!responsiveResult.IsError)
                    responsive = responsiveResult.Value;
            }

            return new ProcessedImageResult(
                Main: main.Value,
                Thumbnail: thumbnail,
                Responsive: responsive,
                OriginalWidth: originalWidth,
                OriginalHeight: originalHeight
            );
        }
        catch (UnknownImageFormatException)
        {
            return ImageErrors.InvalidImage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image processing failed");
            return ImageErrors.ProcessingFailed(ex.Message);
        }
    }

    public async Task<ErrorOr<ImageMetadata>> GetMetadataAsync(
        Stream stream,
        CancellationToken ct = default)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;

            var info = await Image.IdentifyAsync(stream, ct);
            var format = info.Metadata.DecodedImageFormat?.Name?.ToLowerInvariant() ?? "unknown";

            return new ImageMetadata(
                Width: info.Width,
                Height: info.Height,
                Format: format,
                FileSize: stream.Length,
                FileName: string.Empty
            );
        }
        catch (UnknownImageFormatException)
        {
            return ImageErrors.InvalidImage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get metadata");
            return ImageErrors.ProcessingFailed(ex.Message);
        }
    }

    public async Task<ErrorOr<ImageVariant>> ResizeImageAsync(
        Stream stream,
        string fileName,
        int width,
        int height,
        bool crop = false,
        CancellationToken ct = default)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;

            using var image = await Image.LoadAsync(stream, ct);

            var mode = crop ? ResizeMode.Crop : ResizeMode.Max;

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = mode,
                Sampler = KnownResamplers.Lanczos3
            }));

            return await SaveVariantAsync(image, fileName, $"{width}x{height}", _options.Quality, ct);
        }
        catch (UnknownImageFormatException)
        {
            return ImageErrors.InvalidImage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resize failed");
            return ImageErrors.ProcessingFailed(ex.Message);
        }
    }

    public async Task<ErrorOr<ImageVariant>> ConvertFormatAsync(
        Stream stream,
        string fileName,
        string format,
        int quality = 85,
        CancellationToken ct = default)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;

            var formatLower = format.ToLowerInvariant().TrimStart('.');

            if (!new[] { "webp", "jpg", "jpeg", "png" }.Contains(formatLower))
                return ImageErrors.UnsupportedFormat(format);

            using var image = await Image.LoadAsync(stream, ct);

            var outputStream = new MemoryStream();

            switch (formatLower)
            {
                case "webp":
                    await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = quality }, ct);
                    break;
                case "jpg":
                case "jpeg":
                    await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality }, ct);
                    break;
                case "png":
                    await image.SaveAsPngAsync(outputStream, new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression
                    }, ct);
                    break;
            }

            outputStream.Position = 0;

            var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.{formatLower}";

            return new ImageVariant(
                Stream: outputStream,
                FileName: newFileName,
                Width: image.Width,
                Height: image.Height,
                Size: outputStream.Length
            );
        }
        catch (UnknownImageFormatException)
        {
            return ImageErrors.InvalidImage;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Format conversion failed");
            return ImageErrors.ProcessingFailed(ex.Message);
        }
    }

    // Helper methods

    private async Task<ErrorOr<ImageVariant>> SaveVariantAsync(
        Image image,
        string fileName,
        string variant,
        int quality,
        CancellationToken ct)
    {
        try
        {
            var stream = new MemoryStream();
            await image.SaveAsWebpAsync(stream, new WebpEncoder { Quality = quality }, ct);
            stream.Position = 0;

            var newName = $"{Path.GetFileNameWithoutExtension(fileName)}_{variant}.webp";

            return new ImageVariant(
                Stream: stream,
                FileName: newName,
                Width: image.Width,
                Height: image.Height,
                Size: stream.Length
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save variant");
            return ImageErrors.ProcessingFailed(ex.Message);
        }
    }

    private async Task<ErrorOr<ImageVariant>> GenerateThumbnailAsync(
        Image source,
        string fileName,
        CancellationToken ct)
    {
        using var thumb = source.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(_options.ThumbnailSize, _options.ThumbnailSize),
            Mode = ResizeMode.Crop,
            Sampler = KnownResamplers.Lanczos3
        }));

        return await SaveVariantAsync(thumb, fileName, "thumb", 75, ct);
    }

    private async Task<ErrorOr<List<ImageVariant>>> GenerateResponsiveAsync(
        Image source,
        string fileName,
        CancellationToken ct)
    {
        var variants = new List<ImageVariant>();

        foreach (var size in _options.ResponsiveSizes.Where(s => s < source.Width))
        {
            using var resized = source.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(size, 0),
                Mode = ResizeMode.Max
            }));

            var variant = await SaveVariantAsync(resized, fileName, $"w{size}", 80, ct);
            if (!variant.IsError)
                variants.Add(variant.Value);
        }

        return variants;
    }
}