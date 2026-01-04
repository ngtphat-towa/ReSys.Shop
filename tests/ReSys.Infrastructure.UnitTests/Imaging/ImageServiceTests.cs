using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Common.Imaging;
using ReSys.Infrastructure.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ReSys.Infrastructure.UnitTests.Imaging;

public class ImageServiceTests
{
    private readonly ILogger<ImageService> _logger;
    private readonly IOptions<ImageOptions> _options;
    private readonly ImageService _sut;

    public ImageServiceTests()
    {
        _logger = Substitute.For<ILogger<ImageService>>();
        _options = Substitute.For<IOptions<ImageOptions>>();
        _options.Value.Returns(new ImageOptions()); // Default options

        _sut = new ImageService(_logger, _options);
    }

    private Stream CreateTestImageStream(int width, int height)
    {
        var stream = new MemoryStream();
        using (var image = new Image<Rgba32>(width, height))
        {
            image.Mutate(x => x.BackgroundColor(Color.Red));
            image.SaveAsPng(stream);
        }
        stream.Position = 0;
        return stream;
    }

    [Fact(DisplayName = "ProcessAsync: Should resize image when dimensions are provided")]
    public async Task ProcessAsync_ShouldResizeImage_WhenDimensionsProvided()
    {
        // Arrange
        int originalWidth = 1000;
        int originalHeight = 1000;
        int targetWidth = 100;
        
        using var originalStream = CreateTestImageStream(originalWidth, originalHeight);

        // Act
        var result = await _sut.ProcessAsync(
            originalStream, 
            "test.png", 
            maxWidth: targetWidth
        );

        // Assert
        result.IsError.Should().BeFalse();
        var processed = result.Value;
        
        processed.Main.Width.Should().Be(targetWidth);
        processed.Main.Height.Should().Be(targetWidth); // Since it was square
        processed.Main.FileName.Should().Contain("_main.webp");
        
        // Verify the stream is actually a valid image
        processed.Main.Stream.Position = 0;
        var info = await Image.IdentifyAsync(processed.Main.Stream);
        info.Width.Should().Be(targetWidth);
        info.Metadata.DecodedImageFormat?.Name.ToLower().Should().Be("webp");
    }

    [Fact(DisplayName = "ProcessAsync: Should generate thumbnail when requested")]
    public async Task ProcessAsync_ShouldGenerateThumbnail_WhenRequested()
    {
        // Arrange
        using var originalStream = CreateTestImageStream(1000, 1000);

        // Act
        var result = await _sut.ProcessAsync(
            originalStream, 
            "test.png", 
            generateThumbnail: true
        );

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Thumbnail.Should().NotBeNull();
        result.Value.Thumbnail!.FileName.Should().Contain("_thumb.webp");
        result.Value.Thumbnail.Width.Should().Be(300); // Default thumbnail size
    }

    [Fact(DisplayName = "ProcessAsync: Should generate responsive variants when requested")]
    public async Task ProcessAsync_ShouldGenerateResponsiveVariants_WhenRequested()
    {
        // Arrange
        using var originalStream = CreateTestImageStream(2000, 2000);

        // Act
        var result = await _sut.ProcessAsync(
            originalStream, 
            "test.png", 
            generateResponsive: true
        );

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Responsive.Should().NotBeEmpty();
        result.Value.Responsive.Should().Contain(x => x.FileName.Contains("_w320"));
        result.Value.Responsive.Should().Contain(x => x.FileName.Contains("_w640"));
        result.Value.Responsive.Should().Contain(x => x.FileName.Contains("_w1024"));
        result.Value.Responsive.Should().Contain(x => x.FileName.Contains("_w1920"));
    }

    [Fact(DisplayName = "GetMetadataAsync: Should return correct metadata info")]
    public async Task GetMetadataAsync_ShouldReturnCorrectInfo()
    {
        // Arrange
        int width = 50;
        int height = 75;
        using var stream = CreateTestImageStream(width, height);

        // Act
        var result = await _sut.GetMetadataAsync(stream);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Width.Should().Be(width);
        result.Value.Height.Should().Be(height);
        result.Value.Format.Should().Be("png");
    }

    [Fact(DisplayName = "ResizeImageAsync: Should resize and return variant")]
    public async Task ResizeImageAsync_ShouldResizeAndReturnVariant()
    {
        // Arrange
        using var stream = CreateTestImageStream(500, 500);
        int newWidth = 100;
        int newHeight = 50;

        // Act
        var result = await _sut.ResizeImageAsync(stream, "test.png", newWidth, newHeight, crop: true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Width.Should().Be(newWidth);
        result.Value.Height.Should().Be(newHeight);
        result.Value.FileName.Should().Contain($"{newWidth}x{newHeight}");
    }

    [Fact(DisplayName = "ProcessAsync: Should return error when image exceeds size limits")]
    public async Task ProcessAsync_ExceedsLimits_ReturnsTooLargeError()
    {
        // Arrange
        using var stream = CreateTestImageStream(3000, 3000); // Default max is 2048

        // Act
        var result = await _sut.ProcessAsync(stream, "big.png");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Image.TooLarge");
    }

    [Fact(DisplayName = "ProcessAsync: Should resize correctly even for ultra-wide images")]
    public async Task ProcessAsync_UltraWideImage_ResizesCorrectly()
    {
        // Arrange
        using var stream = CreateTestImageStream(2000, 100);
        int maxWidth = 500;

        // Act
        var result = await _sut.ProcessAsync(stream, "wide.png", maxWidth: maxWidth);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Main.Width.Should().Be(maxWidth);
        result.Value.Main.Height.Should().Be(25); // Proportional: 100 * (500/2000)
    }

    [Fact(DisplayName = "ConvertFormatAsync: Should return InvalidImage error for corrupted stream")]
    public async Task ConvertFormatAsync_CorruptedStream_ReturnsProcessingFailed()
    {
        // Arrange
        var bytes = new byte[] { 0x01, 0x02, 0x03 }; // Not a valid image
        using var stream = new MemoryStream(bytes);

        // Act
        var result = await _sut.ConvertFormatAsync(stream, "fake.png", "webp");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Image.Invalid");
    }

    [Fact(DisplayName = "ResizeImageAsync: Should work correctly when target size matches current size")]
    public async Task ResizeImageAsync_ExactlyTheSameSize_WorksCorrectly()
    {
        // Arrange
        using var stream = CreateTestImageStream(100, 100);

        // Act
        var result = await _sut.ResizeImageAsync(stream, "same.png", 100, 100);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Width.Should().Be(100);
    }

    [Fact(DisplayName = "ConvertFormatAsync: Should successfully convert image format")]
    public async Task ConvertFormatAsync_ShouldConvertFormat()
    {
        // Arrange
        using var stream = CreateTestImageStream(100, 100);

        // Act
        var result = await _sut.ConvertFormatAsync(stream, "test.png", "jpg");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().EndWith(".jpg");
        
        result.Value.Stream.Position = 0;
        var info = await Image.IdentifyAsync(result.Value.Stream);
        info.Metadata.DecodedImageFormat?.Name.ToLower().Should().Be("jpeg"); // ImageSharp identifies jpg as jpeg
    }
    
    [Fact(DisplayName = "ConvertFormatAsync: Should return UnsupportedFormat error for invalid target format")]
    public async Task ConvertFormatAsync_ShouldReturnError_ForUnsupportedFormat()
    {
         // Arrange
        using var stream = CreateTestImageStream(100, 100);

        // Act
        var result = await _sut.ConvertFormatAsync(stream, "test.png", "tiff");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Image.UnsupportedFormat");
    }
}
