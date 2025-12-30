using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReSys.Infrastructure.Services;

namespace ReSys.Infrastructure.UnitTests.Services;

public class LocalFileServiceTests
{
    [Fact]
    public async Task SaveFileAsync_ShouldSaveFileToDisk()
    {
        // Arrange
        var config = Substitute.For<IConfiguration>();
        var logger = Substitute.For<ILogger<LocalFileService>>();
        
        config["Storage:LocalPath"].Returns("test_storage");
        
        var service = new LocalFileService(config, logger);
        var content = "Hello World";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var fileName = await service.SaveFileAsync(stream, "test.txt");

        // Assert
        fileName.Should().EndWith("test.txt");
        
        var path = Path.Combine(Directory.GetCurrentDirectory(), "test_storage", fileName);
        File.Exists(path).Should().BeTrue();

        // Cleanup
        if (File.Exists(path)) File.Delete(path);
        if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!);
    }
}
