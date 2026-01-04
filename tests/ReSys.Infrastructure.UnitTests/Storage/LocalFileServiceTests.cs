using System.Text;
using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Common.Storage;
using ReSys.Infrastructure.Storage;
using Xunit;

namespace ReSys.Infrastructure.UnitTests.Storage;

public class LocalFileServiceTests : IDisposable
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
            Subdirectories = new[] { "temp", "products" },
            BufferSize = 4096
        });

        _sut = new LocalFileService(options, logger, _validator, _securityService);
        
        // Default success for validation
        _validator.ValidateAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileUploadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStoragePath))
            try { Directory.Delete(_testStoragePath, true); } catch { }
    }

    // --- SaveFileAsync Tests ---

    [Fact]
    public async Task SaveFileAsync_ValidFile_ReturnsSuccess()
    {
        var content = "test content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var result = await _sut.SaveFileAsync(stream, "test.txt");

        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Contain("test.txt");
        File.Exists(Path.Combine(_testStoragePath, "temp", result.Value.FileId)).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_WithSubdirectory_SavesToCorrectLocation()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var options = new FileUploadOptions(Subdirectory: "products");

        var result = await _sut.SaveFileAsync(stream, "product.png", options);

        result.IsError.Should().BeFalse();
        result.Value.Subdirectory.Should().Be("products");
        File.Exists(Path.Combine(_testStoragePath, "products", result.Value.FileId)).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_EncryptionEnabled_EncryptsFileAndReturnsKey()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("secret"));
        var options = new FileUploadOptions(EncryptFile: true);
        var fakeKey = "fake-key";

        _securityService.EncryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(fakeKey);

        var result = await _sut.SaveFileAsync(stream, "secret.txt", options);

        result.IsError.Should().BeFalse();
        result.Value.EncryptionKey.Should().Be(fakeKey);
        await _securityService.Received(1).EncryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveFileAsync_MalwareScanEnabled_ScansFile()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var options = new FileUploadOptions(ScanForMalware: true);

        _securityService.ScanForMalwareAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.SaveFileAsync(stream, "safe.txt", options);

        result.IsError.Should().BeFalse();
        await _securityService.Received(1).ScanForMalwareAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveFileAsync_ExistingFileNoOverwrite_ReturnsAlreadyExists()
    {
        var fileName = "exist.txt";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        var options = new FileUploadOptions(PreserveOriginalName: true, OverwriteExisting: false);

        await _sut.SaveFileAsync(stream1, fileName, options);
        var result = await _sut.SaveFileAsync(stream2, fileName, options);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.AlreadyExists");
    }

    [Fact]
    public async Task SaveFileAsync_ExistingFileOverwrite_OverwritesFile()
    {
        var fileName = "overwrite.txt";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("old content"));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("new content"));
        var options = new FileUploadOptions(PreserveOriginalName: true, OverwriteExisting: true);

        await _sut.SaveFileAsync(stream1, fileName, options);
        var result = await _sut.SaveFileAsync(stream2, fileName, options);

        result.IsError.Should().BeFalse();
        
        var savedContent = await File.ReadAllTextAsync(Path.Combine(_testStoragePath, "temp", fileName));
        savedContent.Should().Be("new content");
    }

    [Fact]
    public async Task SaveFileAsync_ValidationFails_ReturnsValidationError()
    {
        using var stream = new MemoryStream();
        _validator.ValidateAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<FileUploadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Error.Validation("Fail"));

        var result = await _sut.SaveFileAsync(stream, "bad.txt");

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Fail");
    }

    // --- GetFileStreamAsync Tests ---

    [Fact]
    public async Task GetFileStreamAsync_FileExists_ReturnsStream()
    {
        // Save first
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));
        var saveResult = await _sut.SaveFileAsync(stream, "data.txt");
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileStreamAsync(fileId);

        result.IsError.Should().BeFalse();
        using var reader = new StreamReader(result.Value);
        (await reader.ReadToEndAsync()).Should().Be("data");
    }

    [Fact]
    public async Task GetFileStreamAsync_EncryptedFile_DecryptsAndReturnsStream()
    {
        // Setup mock metadata
        var fileId = "encrypted.txt";
        var content = "secret content";
        var metadata = new FileMetadata(fileId, fileId, fileId, 100, "text/plain", "hash", "temp", DateTime.UtcNow, ".txt", IsEncrypted: true);
        
        Directory.CreateDirectory(Path.Combine(_testStoragePath, ".metadata"));
        Directory.CreateDirectory(Path.Combine(_testStoragePath, "temp"));
        
        await File.WriteAllTextAsync(Path.Combine(_testStoragePath, ".metadata", fileId + ".json"), JsonSerializer.Serialize(metadata));
        await File.WriteAllTextAsync(Path.Combine(_testStoragePath, "temp", fileId), "encrypted_blob");

        _securityService.DecryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(x => 
            {
                var outStream = x.ArgAt<Stream>(1);
                outStream.Write(Encoding.UTF8.GetBytes(content));
                return Result.Success;
            });

        var result = await _sut.GetFileStreamAsync(fileId);

        result.IsError.Should().BeFalse();
        using var reader = new StreamReader(result.Value);
        (await reader.ReadToEndAsync()).Should().Be(content);
        await _securityService.Received(1).DecryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFileStreamAsync_FileNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetFileStreamAsync("missing.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    [Fact]
    public async Task GetFileStreamAsync_DecryptionFails_ReturnsError()
    {
        var fileId = "fail_decrypt.txt";
        var metadata = new FileMetadata(fileId, fileId, fileId, 100, "text/plain", "hash", "temp", DateTime.UtcNow, ".txt", IsEncrypted: true);
        
        Directory.CreateDirectory(Path.Combine(_testStoragePath, ".metadata"));
        Directory.CreateDirectory(Path.Combine(_testStoragePath, "temp"));
        await File.WriteAllTextAsync(Path.Combine(_testStoragePath, ".metadata", fileId + ".json"), JsonSerializer.Serialize(metadata));
        await File.WriteAllTextAsync(Path.Combine(_testStoragePath, "temp", fileId), "data");

        _securityService.DecryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("DecryptFail"));

        var result = await _sut.GetFileStreamAsync(fileId);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("DecryptFail");
    }

    // --- DeleteFileAsync Tests ---

    [Fact]
    public async Task DeleteFileAsync_FileExists_DeletesFileAndMetadata()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("delete me"));
        var saveResult = await _sut.SaveFileAsync(stream, "delete.txt");
        var fileId = saveResult.Value.FileId;

        var result = await _sut.DeleteFileAsync(fileId);

        result.IsError.Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, "temp", fileId)).Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, ".metadata", fileId + ".json")).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_FileNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteFileAsync("missing.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- FileExistsAsync Tests ---

    [Fact]
    public async Task FileExistsAsync_FileExists_ReturnsTrue()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("exists"));
        var saveResult = await _sut.SaveFileAsync(stream, "exists.txt");

        var result = await _sut.FileExistsAsync(saveResult.Value.FileId);
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_FileDoesNotExist_ReturnsFalse()
    {
        var result = await _sut.FileExistsAsync("ghost.txt");
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_CrossSubdirectory_FindsFile()
    {
        // Save to 'products'
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("product"));
        var options = new FileUploadOptions(Subdirectory: "products");
        var saveResult = await _sut.SaveFileAsync(stream, "prod.png", options);
        var fileId = saveResult.Value.FileId;

        // Check exists (should find it by scanning subdirs)
        var result = await _sut.FileExistsAsync(fileId);
        result.Value.Should().BeTrue();
    }

    // --- GetFileMetadataAsync Tests ---

    [Fact]
    public async Task GetFileMetadataAsync_MetadataExists_ReturnsMetadataFromJson()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("meta"));
        var saveResult = await _sut.SaveFileAsync(stream, "meta.txt");
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileMetadataAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.OriginalFileName.Should().Contain("meta.txt");
    }

    [Fact]
    public async Task GetFileMetadataAsync_MetadataMissingFileExists_ReturnsFallbackMetadata()
    {
        // Save file
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fallback"));
        var saveResult = await _sut.SaveFileAsync(stream, "fallback.png");
        var fileId = saveResult.Value.FileId;

        // Delete metadata JSON manually
        File.Delete(Path.Combine(_testStoragePath, ".metadata", fileId + ".json"));

        // Act
        var result = await _sut.GetFileMetadataAsync(fileId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ContentType.Should().Be("image/png"); // Checked fallback logic
        result.Value.Subdirectory.Should().Be("unknown");
    }

    [Fact]
    public async Task GetFileMetadataAsync_FileDoesNotExist_ReturnsNotFound()
    {
        var result = await _sut.GetFileMetadataAsync("gone.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- GetFileHashAsync Tests ---

    [Fact]
    public async Task GetFileHashAsync_HashInMetadata_ReturnsHashFromMetadata()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hash"));
        var saveResult = await _sut.SaveFileAsync(stream, "hash.txt", new FileUploadOptions(GenerateHash: true));
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileHashAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetFileHashAsync_HashMissingInMetadata_CalculatesHash()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("calc hash"));
        // Save without generating hash
        var saveResult = await _sut.SaveFileAsync(stream, "calc.txt", new FileUploadOptions(GenerateHash: false));
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileHashAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
    }

    // --- ListFilesAsync Tests ---

    [Fact]
    public async Task ListFilesAsync_RootDirectory_ReturnsAllFiles()
    {
        // Not directly supported by ListFilesAsync implementation which takes a subdir, 
        // but passing null defaults to root storage path. 
        // However, LocalFileService implementation scans the exact folder passed.
        // Files are saved in subdirectories by default ("temp").
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("1"));
        await _sut.SaveFileAsync(stream, "1.txt", new FileUploadOptions(Subdirectory: "temp"));
        
        var result = await _sut.ListFilesAsync("temp");
        
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task ListFilesAsync_Subdirectory_ReturnsFilesOnlyInSubdirectory()
    {
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("p1"));
        await _sut.SaveFileAsync(stream1, "p1.txt", new FileUploadOptions(Subdirectory: "products"));
        
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("t1"));
        await _sut.SaveFileAsync(stream2, "t1.txt", new FileUploadOptions(Subdirectory: "temp"));

        var result = await _sut.ListFilesAsync("products");

        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(f => f.OriginalFileName.Contains("p1.txt"));
        result.Value.Should().NotContain(f => f.OriginalFileName.Contains("t1.txt"));
    }

    [Fact]
    public async Task ListFilesAsync_InvalidSubdirectory_ReturnsEmptyList()
    {
        var result = await _sut.ListFilesAsync("nonexistent");
        result.IsError.Should().BeFalse(); // Returns empty list, not error
        result.Value.Should().BeEmpty();
    }

    // --- MoveFileAsync Tests ---

    [Fact]
    public async Task MoveFileAsync_ValidMove_MovesFileAndUpdatesMetadata()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("move"));
        var saveResult = await _sut.SaveFileAsync(stream, "move.txt", new FileUploadOptions(Subdirectory: "temp"));
        var fileId = saveResult.Value.FileId;

        var result = await _sut.MoveFileAsync(fileId, "products");

        result.IsError.Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, "temp", fileId)).Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, "products", fileId)).Should().BeTrue();

        var metadata = await _sut.GetFileMetadataAsync(fileId);
        metadata.Value.Subdirectory.Should().Be("products");
    }

    [Fact]
    public async Task MoveFileAsync_SourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.MoveFileAsync("missing.txt", "products");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- CopyFileAsync Tests ---

    [Fact]
    public async Task CopyFileAsync_ValidCopy_CopiesFile()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("copy"));
        var saveResult = await _sut.SaveFileAsync(stream, "copy.txt", new FileUploadOptions(Subdirectory: "temp"));
        var sourceId = saveResult.Value.FileId;

        var result = await _sut.CopyFileAsync(sourceId, "products");

        result.IsError.Should().BeFalse();
        result.Value.FileId.Should().NotBe(sourceId);
        result.Value.Subdirectory.Should().Be("products");
        
        File.Exists(Path.Combine(_testStoragePath, "temp", sourceId)).Should().BeTrue(); // Original remains
        File.Exists(Path.Combine(_testStoragePath, "products", result.Value.FileId)).Should().BeTrue(); // Copy exists
    }

    [Fact]
    public async Task CopyFileAsync_SourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.CopyFileAsync("missing.txt", "products");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }
}
