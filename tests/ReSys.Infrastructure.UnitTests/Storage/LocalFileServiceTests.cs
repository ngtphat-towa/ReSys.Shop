using ErrorOr;


using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using NSubstitute;


using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage;

namespace ReSys.Infrastructure.UnitTests.Storage;

public class LocalFileServiceTests
{
    private readonly LocalFileService _sut;
    private readonly IFileValidator _validator;
    private readonly IFileSecurityService _securityService;
    private readonly string _testStoragePath;

    public LocalFileServiceTests()
    {
        var options = Substitute.For<IOptions<StorageOptions>>();
        var logger = Substitute.For<ILogger<LocalFileService>>();
        _validator = Substitute.For<IFileValidator>();
        _securityService = Substitute.For<IFileSecurityService>();

        _testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "test_storage_" + Guid.NewGuid());
        options.Value.Returns(new StorageOptions 
        { 
            LocalPath = _testStoragePath,
            Subdirectories = new[] { "temp", "products" }
        });

        _sut = new LocalFileService(options, logger, _validator, _securityService);
    }

    [Fact]
    public async Task SaveFileAsync_Success_ReturnsResult()
    {
        // Arrange
        var content = "Hello World";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        
        _validator.ValidateAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileUploadOptions>())
            .Returns(Result.Success);

        var uploadOptions = new FileUploadOptions();

        // Act
        var result = await _sut.SaveFileAsync(stream, "test.txt", uploadOptions);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Contain("test.txt");
        
        // Verify file exists
        var exists = await _sut.FileExistsAsync(result.Value.FileId);
        exists.IsError.Should().BeFalse();
        exists.Value.Should().BeTrue();

        Cleanup();
    }

    [Fact]
    public async Task SaveFileAsync_ValidationFails_ReturnsError()
    {
        // Arrange
        using var stream = new MemoryStream();
        _validator.ValidateAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileUploadOptions>())
            .Returns(Error.Validation("InvalidFile"));

        // Act
        var result = await _sut.SaveFileAsync(stream, "test.txt");

        // Assert
        result.IsError.Should().BeTrue();
    }

    private void Cleanup()
    {
        if (Directory.Exists(_testStoragePath))
            try { Directory.Delete(_testStoragePath, true); } catch { }
    }
}