using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using ReSys.Infrastructure.Storage.Options;
using ReSys.Infrastructure.Storage.Services;

namespace ReSys.Infrastructure.UnitTests.Storage;

public class FileSecurityServiceTests
{
    private readonly FileSecurityService _sut;
    private readonly IOptions<StorageOptions> _options;

    public FileSecurityServiceTests()
    {
        _options = Substitute.For<IOptions<StorageOptions>>();
        _options.Value.Returns(new StorageOptions
        {
            Security = new SecurityOptions { EncryptionKey = "SuperSecretKey123!" },
            BufferSize = 1024
        });
        
        var logger = Substitute.For<ILogger<FileSecurityService>>();
        _sut = new FileSecurityService(_options, logger);
    }

    [Fact]
    public async Task EncryptAndDecrypt_ShouldRoundtripData()
    {
        // Arrange
        var originalText = "This is a secret message.";
        var originalBytes = Encoding.UTF8.GetBytes(originalText);
        
        using var inputStream = new MemoryStream(originalBytes);
        using var encryptedStream = new MemoryStream();

        // Act - Encrypt
        var encryptResult = await _sut.EncryptFileAsync(inputStream, encryptedStream, TestContext.Current.CancellationToken);
        
        // Assert - Encrypt
        encryptResult.IsError.Should().BeFalse();
        
        // Decrypt
        encryptedStream.Position = 0;
        using var decryptedStream = new MemoryStream();
        var decryptResult = await _sut.DecryptFileAsync(encryptedStream, decryptedStream, "ignored", TestContext.Current.CancellationToken);

        decryptResult.IsError.Should().BeFalse();
        Encoding.UTF8.GetString(decryptedStream.ToArray()).Should().Be(originalText);
    }

    [Fact]
    public async Task EncryptFileAsync_MissingKey_ReturnsError()
    {
        // Arrange
        _options.Value.Returns(new StorageOptions { Security = new SecurityOptions { EncryptionKey = "" } });
        var sut = new FileSecurityService(_options, Substitute.For<ILogger<FileSecurityService>>());
        
        using var input = new MemoryStream();
        using var output = new MemoryStream();

        // Act
        var result = await sut.EncryptFileAsync(input, output, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.EncryptionNotConfigured");
    }

    [Fact]
    public async Task ScanForMalwareAsync_CleanFile_ReturnsTrue()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Just some safe content"));

        // Act
        var result = await _sut.ScanForMalwareAsync(stream, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ScanForMalwareAsync_EicarFile_ReturnsFalse()
    {
        // Arrange
        var eicar = @"X5O!P%@AP[4\PZX54(P^)7CC)7}";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(eicar));

        // Act
        var result = await _sut.ScanForMalwareAsync(stream, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
    }
}
