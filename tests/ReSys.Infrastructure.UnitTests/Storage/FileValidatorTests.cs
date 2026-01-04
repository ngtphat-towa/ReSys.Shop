using System.Text;


using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using NSubstitute;


using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage;

namespace ReSys.Infrastructure.UnitTests.Storage;

public class FileValidatorTests
{
    private readonly FileValidator _sut;
    private readonly IOptions<StorageOptions> _options;

    public FileValidatorTests()
    {
        _options = Substitute.For<IOptions<StorageOptions>>();
        _options.Value.Returns(new StorageOptions());
        var logger = Substitute.For<ILogger<FileValidator>>();
        _sut = new FileValidator(_options, logger);
    }

    [Fact]
    public async Task ValidateAsync_ValidTextFile_ReturnsSuccess()
    {
        // Arrange
        var content = "Hello World";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_EmptyFileName_ReturnsError()
    {
        // Arrange
        using var stream = new MemoryStream();
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "", options);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.InvalidFileName");
    }

    [Fact]
    public async Task ValidateAsync_EmptyFileStream_ReturnsError()
    {
        // Arrange
        using var stream = new MemoryStream(); // Empty
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.Empty");
    }

    [Fact]
    public async Task ValidateAsync_InvalidExtension_ReturnsError()
    {
        // Arrange
        var content = "test";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var options = new FileUploadOptions(AllowedExtensions: new[] { ".jpg" });

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.InvalidExtension");
    }

    [Fact]
    public async Task ValidateAsync_FileSizeExceeded_ReturnsError()
    {
        // Arrange
        var content = new byte[1024]; // 1KB
        using var stream = new MemoryStream(content);
        var options = new FileUploadOptions(MaxFileSize: 100); // 100 Bytes

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.TooLarge");
    }
}