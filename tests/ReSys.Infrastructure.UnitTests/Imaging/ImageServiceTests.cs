using ReSys.Infrastructure.Imaging;


using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ReSys.Infrastructure.UnitTests.Imaging;

public class ImageServiceTests
{
    private readonly ImageService _sut; // System Under Test

    public ImageServiceTests()
    {
        _sut = new ImageService();
    }

    private Stream CreateTestImageStream(int width, int height)
    {
        var stream = new MemoryStream();
        using (var image = new Image<Rgba32>(width, height))
        {
            // Just fill it with a color so it's a valid image
            image.Mutate(x => x.BackgroundColor(Color.Red));
            image.SaveAsPng(stream);
        }
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task ProcessImageAsync_ShouldResizeImage_WhenDimensionsProvided()
    {
        // Arrange
        int originalWidth = 1000;
        int originalHeight = 1000;
        int targetWidth = 100;
        
        using var originalStream = CreateTestImageStream(originalWidth, originalHeight);

        // Act
        var (processedStream, metadata) = await _sut.ProcessImageAsync(
            originalStream, 
            "test.png", 
            maxWidth: targetWidth
        );

        // Assert
        metadata.Width.Should().Be(targetWidth);
        metadata.Height.Should().Be(targetWidth); // Since it was square
        metadata.Format.Should().Be("webp"); // Default conversion
        
        // Verify the stream is actually a valid image
        processedStream.Position = 0;
        var info = await Image.IdentifyAsync(processedStream);
        info.Width.Should().Be(targetWidth);
        info.Metadata.DecodedImageFormat?.Name.ToLower().Should().Be("webp");
    }

    [Fact]
    public async Task ProcessImageAsync_ShouldConvertFormatToWebP_WhenNoResizeNeeded()
    {
        // Arrange
        using var originalStream = CreateTestImageStream(100, 100);

        // Act
        var (processedStream, metadata) = await _sut.ProcessImageAsync(
            originalStream, 
            "test.png"
        );

        // Assert
        metadata.Format.Should().Be("webp");
        metadata.FileName.Should().EndWith(".webp");
        
        processedStream.Position = 0;
        var info = await Image.IdentifyAsync(processedStream);
        info.Metadata.DecodedImageFormat?.Name.ToLower().Should().Be("webp");
    }

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnCorrectInfo_WithoutModifyingStream()
    {
        // Arrange
        int width = 50;
        int height = 75;
        using var stream = CreateTestImageStream(width, height);
        long originalPosition = stream.Position;

        // Act
        var metadata = await _sut.GetMetadataAsync(stream, "photo.png");

        // Assert
        metadata.Width.Should().Be(width);
        metadata.Height.Should().Be(height);
        metadata.Format.Should().Be("png"); // Input was PNG
        metadata.FileName.Should().Be("photo.png");
        
        // Ensure stream is still usable/reset (or at least check implementation behavior)
        // The implementation resets position to 0 if seekable.
        // If we want to verify it didn't close it, we can read it.
        stream.CanRead.Should().BeTrue();
    }
}