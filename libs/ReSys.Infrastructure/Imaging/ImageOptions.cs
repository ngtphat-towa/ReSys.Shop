using System.ComponentModel.DataAnnotations;

namespace ReSys.Infrastructure.Imaging;

public sealed class ImageOptions
{
    public const string SectionName = "Image";
    public static ImageOptions Default => new();
    [Range(1, 8192)]
    public int MaxWidth { get; set; } = 2048;
    [Range(1, 8192)]
    public int MaxHeight { get; set; } = 2048;
    [Range(1, 1024)]
    public int ThumbnailSize { get; set; } = 300;
    [Range(1, 100)]
    public int Quality { get; set; } = 85;
    [Required]
    public int[] ResponsiveSizes { get; set; } = new[] { 320, 640, 1024, 1920 };
}
