using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Common.Storage;
using ReSys.Api.Features.Files;
using Xunit;

namespace ReSys.Api.IntegrationTests.Features.Files;

[Collection("Shared Database")]
public class FileUploadTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/files/image: Should upload image and convert to WebP")]
    public async Task UploadImage_ValidPng_ReturnsWebpMetadata()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
        var assetPath = Path.Combine(projectRoot, "tests", "TestAssets", "sample.png");
        
        if (!File.Exists(assetPath))
        {
            assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets", "sample.png");
        }

        var imageBytes = await File.ReadAllBytesAsync(assetPath);
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        
        content.Add(fileContent, "file", "test-image.png");

        // Act
        var response = await Client.PostAsync("/api/files/image", content);
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<FileUploadResponse>>(responseString, JsonSettings);
        apiResponse.Should().NotBeNull();
        
        var data = apiResponse!.Data;
        Assert.NotNull(data);
        data!.SavedName.Should().EndWith(".webp");
        data.Format.Should().Be("webp");
        data.Width.Should().BeLessThanOrEqualTo(1200); // FilesModule resizes to 1200
    }

    [Fact(DisplayName = "GET /api/files/{path}/metadata: Should return metadata for existing file")]
    public async Task GetFileMetadata_ExistingFile_ReturnsMetadata()
    {
        // Arrange - Upload a file first
        using var content = new MultipartFormDataContent();
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
        var assetPath = Path.Combine(projectRoot, "tests", "TestAssets", "sample.png");
        if (!File.Exists(assetPath)) assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets", "sample.png");

        var imageBytes = await File.ReadAllBytesAsync(assetPath);
        content.Add(new ByteArrayContent(imageBytes), "file", "metadata-test.png");

        var uploadResponse = await Client.PostAsync("/api/files/image", content);
        var uploadResult = JsonConvert.DeserializeObject<ApiResponse<FileUploadResponse>>(await uploadResponse.Content.ReadAsStringAsync(), JsonSettings);
        var fileUrl = uploadResult!.Data!.Url; // e.g. /api/files/products/guid_main.webp
        var relativePath = fileUrl.Replace("/api/files/", "");

        // Act
        var response = await Client.GetAsync($"/api/files/meta/{relativePath}");
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<FileMetadata>>(responseString, JsonSettings);
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.OriginalFileName.Should().Contain("metadata-test");
        apiResponse.Data!.OriginalFileName.Should().EndWith(".webp");
        apiResponse.Data.ContentType.Should().Be("image/webp");
    }

    [Fact(DisplayName = "FileUploadOptions: Should combine Subdirectory and Subdirectories correctly")]
    public void FileUploadOptions_GetCombinedSubdirectory_ReturnsCorrectPath()
    {
        // Arrange
        var options = new FileUploadOptions(new[] { "nested", "level2" });

        // Act
        var result = options.GetPath();

        // Assert
        result.Should().Be("nested/level2");
    }

    [Fact(DisplayName = "POST /api/files/image: Should sanitize path traversal names")]
    public async Task UploadImage_PathTraversal_SanitizesName()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
        var assetPath = Path.Combine(projectRoot, "tests", "TestAssets", "sample.png");
        if (!File.Exists(assetPath)) assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets", "sample.png");

        var imageBytes = await File.ReadAllBytesAsync(assetPath);
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        
        // Malicious filename
        content.Add(fileContent, "file", "../../../hacker.png");

        // Act
        var response = await Client.PostAsync("/api/files/image", content);
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<FileUploadResponse>>(responseString, JsonSettings);
        apiResponse!.Data!.SavedName.Should().NotContain("..");
    }

    [Fact(DisplayName = "POST /api/files/image: Should return BadRequest for invalid file")]
    public async Task UploadImage_InvalidFile_ReturnsBadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        var bytes = "not an image"u8.ToArray();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(fileContent, "file", "fake.png");

        // Act
        var response = await Client.PostAsync("/api/files/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
