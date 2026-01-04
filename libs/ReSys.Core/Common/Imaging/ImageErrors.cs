namespace ReSys.Core.Common.Imaging;

using ErrorOr;

public static class ImageErrors
{
    public static Error InvalidImage => Error.Validation(
        "Image.Invalid",
        "The file is not a valid image");

    public static Error UnsupportedFormat(string format) => Error.Validation(
        "Image.UnsupportedFormat",
        $"Image format '{format}' is not supported");

    public static Error ProcessingFailed(string reason) => Error.Failure(
        "Image.ProcessingFailed",
        $"Image processing failed: {reason}");

    public static Error TooLarge(int width, int height, int maxWidth, int maxHeight) => Error.Validation(
        "Image.TooLarge",
        $"Image ({width}x{height}) exceeds max ({maxWidth}x{maxHeight})");
}
