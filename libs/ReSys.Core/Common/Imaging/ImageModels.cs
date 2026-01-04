namespace ReSys.Core.Common.Imaging;

public sealed record ImageMetadata(
    int Width,
    int Height,
    string Format,
    long FileSize,
    string FileName);

public sealed record ImageVariant(
    Stream Stream,
    string FileName,
    int Width,
    int Height,
    long Size);

public sealed record ProcessedImageResult(
    ImageVariant Main,
    ImageVariant? Thumbnail,
    List<ImageVariant> Responsive,
    int OriginalWidth,
    int OriginalHeight);
