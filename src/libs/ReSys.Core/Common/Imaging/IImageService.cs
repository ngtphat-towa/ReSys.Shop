namespace ReSys.Core.Common.Imaging;

using ErrorOr;

public interface IImageService
{
    Task<ErrorOr<ProcessedImageResult>> ProcessAsync(
        Stream stream,
        string fileName,
        int? maxWidth = null,
        int? maxHeight = null,
        bool generateThumbnail = true,
        bool generateResponsive = false,
        CancellationToken ct = default);

    Task<ErrorOr<ImageMetadata>> GetMetadataAsync(
        Stream stream,
        CancellationToken ct = default);

    Task<ErrorOr<ImageVariant>> ResizeImageAsync(
        Stream stream,
        string fileName,
        int width,
        int height,
        bool crop = false,
        CancellationToken ct = default);

    Task<ErrorOr<ImageVariant>> ConvertFormatAsync(
        Stream stream,
        string fileName,
        string format,
        int quality = 85,
        CancellationToken ct = default);
}
