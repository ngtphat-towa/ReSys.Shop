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

    [Fact(DisplayName = "SaveFileAsync: Should successfully save a valid file")]
    public async Task SaveFileAsync_ValidFile_ReturnsSuccess()
    {
        var content = "test content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var result = await _sut.SaveFileAsync(stream, "test.txt");

        result.IsError.Should().BeFalse();
        result.Value.FileName.Should().Contain("test.txt");
        File.Exists(Path.Combine(_testStoragePath, "temp", result.Value.FileId)).Should().BeTrue();
    }

    [Fact(DisplayName = "SaveFileAsync: Should save to requested subdirectory")]
    public async Task SaveFileAsync_WithSubdirectory_SavesToCorrectLocation()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var options = new FileUploadOptions("products");

        var result = await _sut.SaveFileAsync(stream, "product.png", options);

        result.IsError.Should().BeFalse();
        result.Value.Subdirectory.Should().Be("products");
        File.Exists(Path.Combine(_testStoragePath, "products", result.Value.FileId)).Should().BeTrue();
    }

    [Fact(DisplayName = "SaveFileAsync: Should encrypt file when requested")]
    public async Task SaveFileAsync_EncryptionEnabled_EncryptsFileAndReturnsKey()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("secret"));
        var options = new FileUploadOptions { EncryptFile = true };
        var fakeKey = "fake-key";

        _securityService.EncryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(fakeKey);

        var result = await _sut.SaveFileAsync(stream, "secret.txt", options);

        result.IsError.Should().BeFalse();
        result.Value.EncryptionKey.Should().Be(fakeKey);
        await _securityService.Received(1).EncryptFileAsync(Arg.Any<Stream>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "SaveFileAsync: Should scan for malware when requested")]
    public async Task SaveFileAsync_MalwareScanEnabled_ScansFile()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var options = new FileUploadOptions { ScanForMalware = true };

        _securityService.ScanForMalwareAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.SaveFileAsync(stream, "safe.txt", options);

        result.IsError.Should().BeFalse();
        await _securityService.Received(1).ScanForMalwareAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "SaveFileAsync: Should return error when file exists and overwrite is disabled")]
    public async Task SaveFileAsync_ExistingFileNoOverwrite_ReturnsAlreadyExists()
    {
        var fileName = "exist.txt";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));
        var options = new FileUploadOptions { PreserveOriginalName = true, OverwriteExisting = false };

        await _sut.SaveFileAsync(stream1, fileName, options);
        var result = await _sut.SaveFileAsync(stream2, fileName, options);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.AlreadyExists");
    }

    [Fact(DisplayName = "SaveFileAsync: Should overwrite file when requested")]
    public async Task SaveFileAsync_ExistingFileOverwrite_OverwritesFile()
    {
        var fileName = "overwrite.txt";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("old content"));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("new content"));
        var options = new FileUploadOptions { PreserveOriginalName = true, OverwriteExisting = true };

        await _sut.SaveFileAsync(stream1, fileName, options);
        var result = await _sut.SaveFileAsync(stream2, fileName, options);

        result.IsError.Should().BeFalse();
        
        var savedContent = await File.ReadAllTextAsync(Path.Combine(_testStoragePath, "temp", fileName));
        savedContent.Should().Be("new content");
    }

    [Fact(DisplayName = "SaveFileAsync: Should return error when validation fails")]
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

    [Fact(DisplayName = "GetFileStreamAsync: Should return file stream when file exists")]
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

    [Fact(DisplayName = "GetFileStreamAsync: Should decrypt and return stream for encrypted files")]
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

    [Fact(DisplayName = "GetFileStreamAsync: Should return NotFound error for missing files")]
    public async Task GetFileStreamAsync_FileNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetFileStreamAsync("missing.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    [Fact(DisplayName = "GetFileStreamAsync: Should return error when decryption fails")]
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

    [Fact(DisplayName = "DeleteFileAsync: Should delete file and its metadata")]
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

    [Fact(DisplayName = "DeleteFileAsync: Should return NotFound error for missing files")]
    public async Task DeleteFileAsync_FileNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteFileAsync("missing.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- FileExistsAsync Tests ---

    [Fact(DisplayName = "FileExistsAsync: Should return true when file exists")]
    public async Task FileExistsAsync_FileExists_ReturnsTrue()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("exists"));
        var saveResult = await _sut.SaveFileAsync(stream, "exists.txt");

        var result = await _sut.FileExistsAsync(saveResult.Value.FileId);
        result.Value.Should().BeTrue();
    }

    [Fact(DisplayName = "FileExistsAsync: Should return false when file does not exist")]
    public async Task FileExistsAsync_FileDoesNotExist_ReturnsFalse()
    {
        var result = await _sut.FileExistsAsync("ghost.txt");
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "FileExistsAsync: Should find file across subdirectories")]
    public async Task FileExistsAsync_CrossSubdirectory_FindsFile()
    {
        // Save to 'products'
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("product"));
        var options = new FileUploadOptions("products");
        var saveResult = await _sut.SaveFileAsync(stream, "prod.png", options);
        var fileId = saveResult.Value.FileId;

        // Check exists (should find it by scanning subdirs)
        var result = await _sut.FileExistsAsync(fileId);
        result.Value.Should().BeTrue();
    }

    // --- GetFileMetadataAsync Tests ---

    [Fact(DisplayName = "GetFileMetadataAsync: Should return metadata from JSON file")]
    public async Task GetFileMetadataAsync_MetadataExists_ReturnsMetadataFromJson()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("meta"));
        var saveResult = await _sut.SaveFileAsync(stream, "meta.txt");
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileMetadataAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.OriginalFileName.Should().Contain("meta.txt");
    }

    [Fact(DisplayName = "GetFileMetadataAsync: Should return fallback metadata when JSON is missing")]
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

    [Fact(DisplayName = "GetFileMetadataAsync: Should return NotFound for non-existent files")]
    public async Task GetFileMetadataAsync_FileDoesNotExist_ReturnsNotFound()
    {
        var result = await _sut.GetFileMetadataAsync("gone.txt");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- GetFileHashAsync Tests ---

    [Fact(DisplayName = "GetFileHashAsync: Should return hash from metadata if available")]
    public async Task GetFileHashAsync_HashInMetadata_ReturnsHashFromMetadata()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hash"));
        var saveResult = await _sut.SaveFileAsync(stream, "hash.txt", new FileUploadOptions { GenerateHash = true });
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileHashAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "GetFileHashAsync: Should calculate hash if missing from metadata")]
    public async Task GetFileHashAsync_HashMissingInMetadata_CalculatesHash()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("calc hash"));
        // Save without generating hash
        var saveResult = await _sut.SaveFileAsync(stream, "calc.txt", new FileUploadOptions { GenerateHash = false });
        var fileId = saveResult.Value.FileId;

        var result = await _sut.GetFileHashAsync(fileId);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
    }

    // --- ListFilesAsync Tests ---

    [Fact(DisplayName = "ListFilesAsync: Should return all files in a directory")]
    public async Task ListFilesAsync_RootDirectory_ReturnsAllFiles()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("1"));
        await _sut.SaveFileAsync(stream, "1.txt", new FileUploadOptions("temp"));
        
        var result = await _sut.ListFilesAsync("temp");
        
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact(DisplayName = "ListFilesAsync: Should return files only within specified subdirectory")]
    public async Task ListFilesAsync_Subdirectory_ReturnsFilesOnlyInSubdirectory()
    {
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes("p1"));
        await _sut.SaveFileAsync(stream1, "p1.txt", new FileUploadOptions("products"));
        
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes("t1"));
        await _sut.SaveFileAsync(stream2, "t1.txt", new FileUploadOptions("temp"));

        var result = await _sut.ListFilesAsync("products");

        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(f => f.OriginalFileName.Contains("p1.txt"));
        result.Value.Should().NotContain(f => f.OriginalFileName.Contains("t1.txt"));
    }

    [Fact(DisplayName = "ListFilesAsync: Should return empty list for invalid subdirectory")]
    public async Task ListFilesAsync_InvalidSubdirectory_ReturnsEmptyList()
    {
        var result = await _sut.ListFilesAsync("nonexistent");
        result.IsError.Should().BeFalse(); // Returns empty list, not error
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "SaveFileAsync: Should sanitize dangerous characters in filenames")]
    public async Task SaveFileAsync_DangerousCharacters_SanitizesFileName()
    {
        // Arrange
        var content = "safe";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        // Validator blocks "..", but we can test sanitization of other chars like ':' or '*'
        var maliciousName = "test:file*name.txt";

        // Act
        var result = await _sut.SaveFileAsync(stream, maliciousName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileId.Should().NotContain(":");
        result.Value.FileId.Should().NotContain("*");
        result.Value.FileId.Should().Contain("test_file_name.txt");
    }

    [Fact(DisplayName = "SaveFileAsync: Should successfully save to deep nested subdirectories")]
    public async Task SaveFileAsync_DeepNested_SavesSuccessfully()
    {
        // Arrange
        using var stream = new MemoryStream("content"u8.ToArray());
        var options = new FileUploadOptions(Subdirectories: new[] { "a", "b", "c" });

        // Act
        var result = await _sut.SaveFileAsync(stream, "test.txt", options);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Subdirectory.Should().Be("a/b/c");
        var expectedPath = Path.Combine(_testStoragePath, "a", "b", "c", result.Value.FileId);
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact(DisplayName = "FindFilePath: Should recursively find files in deep nested directories")]
    public async Task FindFilePath_DeepNested_FindsByFileNameOnly()
    {
        // Arrange
        using var stream = new MemoryStream("content"u8.ToArray());
        var options = new FileUploadOptions(new[] { "level1", "level2" });
        var saved = await _sut.SaveFileAsync(stream, "deep.txt", options);
        var fileId = saved.Value.FileId;

        // Act - Search by filename only (no path)
        var exists = await _sut.FileExistsAsync(fileId);

        // Assert
        exists.Value.Should().BeTrue("because the service should scan recursively");
    }

    [Fact(DisplayName = "GetFileMetadataAsync: Should find metadata when ID includes subdirectory")]
    public async Task GetFileMetadataAsync_WithSubdirInId_FindsMetadata()
    {
        // Arrange
        using var stream = new MemoryStream("content"u8.ToArray());
        var options = new FileUploadOptions("nested");
        var saved = await _sut.SaveFileAsync(stream, "meta.txt", options);
        var pathInclusiveId = $"nested/{saved.Value.FileId}";

        // Act
        var result = await _sut.GetFileMetadataAsync(pathInclusiveId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileId.Should().Be(saved.Value.FileId);
    }

    [Fact(DisplayName = "SaveFileAsync: Should successfully save an empty file stream")]
    public async Task SaveFileAsync_EmptyStream_SavesSuccessfully()
    {
        // Arrange
        using var stream = new MemoryStream(Array.Empty<byte>());

        // Act
        var result = await _sut.SaveFileAsync(stream, "empty.txt");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileSize.Should().Be(0);
    }

    [Fact(DisplayName = "MoveFileAsync: Should successfully move file and update its metadata")]
    public async Task MoveFileAsync_ValidMove_MovesFileAndUpdatesMetadata()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("move"));
        var saveResult = await _sut.SaveFileAsync(stream, "move.txt", new FileUploadOptions("temp"));
        var fileId = saveResult.Value.FileId;

        var result = await _sut.MoveFileAsync(fileId, "products");

        result.IsError.Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, "temp", fileId)).Should().BeFalse();
        File.Exists(Path.Combine(_testStoragePath, "products", fileId)).Should().BeTrue();

        var metadata = await _sut.GetFileMetadataAsync(fileId);
        metadata.Value.Subdirectory.Should().Be("products");
    }

    [Fact(DisplayName = "MoveFileAsync: Should return NotFound for missing source file")]
    public async Task MoveFileAsync_SourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.MoveFileAsync("missing.txt", "products");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }

    // --- CopyFileAsync Tests ---

    [Fact(DisplayName = "CopyFileAsync: Should successfully copy file to a new location")]
    public async Task CopyFileAsync_ValidCopy_CopiesFile()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("copy"));
        var saveResult = await _sut.SaveFileAsync(stream, "copy.txt", new FileUploadOptions("temp"));
        var sourceId = saveResult.Value.FileId;

        var result = await _sut.CopyFileAsync(sourceId, "products");

        result.IsError.Should().BeFalse();
        result.Value.FileId.Should().NotBe(sourceId);
        result.Value.Subdirectory.Should().Be("products");
        
        File.Exists(Path.Combine(_testStoragePath, "temp", sourceId)).Should().BeTrue(); // Original remains
        File.Exists(Path.Combine(_testStoragePath, "products", result.Value.FileId)).Should().BeTrue(); // Copy exists
    }

    [Fact(DisplayName = "CopyFileAsync: Should return NotFound for missing source file")]
    public async Task CopyFileAsync_SourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.CopyFileAsync("missing.txt", "products");
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("File.NotFound");
    }
}
