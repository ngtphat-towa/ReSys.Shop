using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage.Options;
using ReSys.Infrastructure.Storage.Validators;

namespace ReSys.Infrastructure.UnitTests.Storage;

public class FileValidatorTests
{
    private readonly FileValidator _sut;
    private readonly IOptions<StorageOptions> _options;

    public FileValidatorTests()
    {
        _options = Substitute.For<IOptions<StorageOptions>>();
        _options.Value.Returns(new StorageOptions
        {
            Security = new SecurityOptions 
            { 
                DangerousExtensions = new[] { ".exe", ".bat", ".sh" },
                ValidateFileSignatures = true
            },
            MaxFileSize = 1024 * 1024 // 1MB
        });
        
        var logger = Substitute.For<ILogger<FileValidator>>();
        _sut = new FileValidator(_options, logger);
    }

    [Fact(DisplayName = "ValidateAsync: Should return success for valid text file")]
    public async Task ValidateAsync_ValidTextFile_ReturnsSuccess()
    {
        // Arrange
        var content = "Hello World";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact(DisplayName = "ValidateAsync: Should return error for empty filename")]
    public async Task ValidateAsync_EmptyFileName_ReturnsError()
    {
        // Arrange
        using var stream = new MemoryStream();
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.InvalidFileName");
    }

    [Fact(DisplayName = "ValidateAsync: Should return error for empty file stream")]
    public async Task ValidateAsync_EmptyFileStream_ReturnsError()
    {
        // Arrange
        using var stream = new MemoryStream(); // Empty
        var options = new FileUploadOptions();

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.Empty");
    }

    [Fact(DisplayName = "ValidateAsync: Should return error for disallowed extension")]
    public async Task ValidateAsync_InvalidExtension_ReturnsError()
    {
        // Arrange
        var content = "test";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var options = new FileUploadOptions(AllowedExtensions: new[] { ".jpg" });

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.InvalidExtension");
    }

    [Fact(DisplayName = "ValidateAsync: Should return error when max file size is exceeded")]
    public async Task ValidateAsync_FileSizeExceeded_ReturnsError()
    {
        // Arrange
        var content = new byte[1024]; // 1KB
        using var stream = new MemoryStream(content);
        var options = new FileUploadOptions(MaxFileSize: 100); // 100 Bytes

        // Act
        var result = await _sut.ValidateAsync(stream, "test.txt", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.TooLarge");
    }

    [Fact(DisplayName = "ValidateAsync: Should return error for filenames containing dangerous characters")]
    public async Task ValidateAsync_DangerousCharacters_ReturnsInvalidFileName()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var options = new FileUploadOptions();
        var dangerousNames = new[] { "../test.txt", "test..txt", "test<>.txt", "test|pipe.txt" };

        foreach (var name in dangerousNames)
        {
            // Act
            var result = await _sut.ValidateAsync(stream, name, options, TestContext.Current.CancellationToken);

            // Assert
            result.IsError.Should().BeTrue($"Name {name} should be invalid");
            result.FirstError.Code.Should().Be("File.InvalidFileName");
        }
    }

    [Fact(DisplayName = "ValidateAsync: Should return error for dangerous file extensions")]
    public async Task ValidateAsync_DangerousExtension_ReturnsDangerousExtension()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("malware"));
        var options = new FileUploadOptions();
        
        // Act
        var result = await _sut.ValidateAsync(stream, "malware.exe", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.DangerousExtension");
    }

    [Fact(DisplayName = "ValidateAsync: Should return error when file signature does not match extension")]
    public async Task ValidateAsync_SignatureMismatch_ReturnsSignatureMismatch()
    {
        // Arrange
        // Fake a PNG file with random text content
        var content = "This is not a PNG";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var options = new FileUploadOptions(ValidateContent: true);

        // Act
        var result = await _sut.ValidateAsync(stream, "fake.png", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.SignatureMismatch");
    }

    [Fact(DisplayName = "ValidateAsync: Should return success when file signature matches extension")]
    public async Task ValidateAsync_ValidSignature_ReturnsSuccess()
    {
        // Arrange
        // Correct PNG header: 89 50 4E 47
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = new MemoryStream(pngBytes);
        var options = new FileUploadOptions(ValidateContent: true);

        // Act
        var result = await _sut.ValidateAsync(stream, "real.png", options, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }
}
