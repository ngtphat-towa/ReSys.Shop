using System.Net;
using System.Net.Http.Headers;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Testing.Examples.Common;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Storage;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class UpdateExampleImageTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/testing/examples/{id}/image: Should upload image and update Example URL")]
    public async Task UpdateImage_ValidFile_ReturnsUpdatedExample()
    {
        // Arrange
        var ExampleId = Guid.NewGuid();
        Context.Set<Example>().Add(new Example 
        { 
            Id = ExampleId, 
            Name = $"ImageTest_{Guid.NewGuid()}", 
            Description = "Desc", 
            Price = 10,
            CreatedAt = DateTimeOffset.UtcNow,
            ImageUrl = "",
            Status = ExampleStatus.Draft,
            HexColor = "#000000"
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var content = new MultipartFormDataContent();
        
        // Find real PNG asset
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".."));
        var assetPath = Path.Combine(projectRoot, "tests", "TestAssets", "sample.png");
        
        if (!File.Exists(assetPath))
        {
            // Fallback for bin execution
            assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets", "sample.png");
        }

        if (!File.Exists(assetPath))
        {
             throw new FileNotFoundException($"Test asset not found at {assetPath}. Current directory: {Directory.GetCurrentDirectory()}");
        }

        var imageBytes = await File.ReadAllBytesAsync(assetPath, TestContext.Current.CancellationToken);
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        
        content.Add(fileContent, "image", "test-image.png");

        // Act
        var response = await Client.PostAsync($"/api/testing/examples/{ExampleId}/image", content, TestContext.Current.CancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Request Failed: {response.StatusCode} \nContent: {responseString}");
        }

        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(responseString, JsonSettings);
        var result = apiResponse!.Data;

        result!.ImageUrl.Should().Contain("/examples/");
        result!.ImageUrl.Should().Contain(".webp");
        result.Id.Should().Be(ExampleId);

        // Verify DB update
        ((DbContext)Context).ChangeTracker.Clear();
        var dbExample = await Context.Set<Example>().FindAsync(new object?[] { ExampleId, TestContext.Current.CancellationToken }, TestContext.Current.CancellationToken);

        dbExample.Should().NotBeNull();

        dbExample!.ImageUrl!.Should().Contain("/examples/");
        dbExample!.ImageUrl!.Should().Contain(".webp");

        // Verify file storage
        var fileName = dbExample.ImageUrl!.Split('/').Last();
        var fullPathId = $"examples/{fileName}";
        
        using var scope = Factory.Services.CreateScope();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var existsResult = await fileService.FileExistsAsync(fullPathId, TestContext.Current.CancellationToken);
        
        existsResult.IsError.Should().BeFalse();
        existsResult.Value.Should().BeTrue("because the file was successfully saved");

        // Verify file can be loaded via API
        var getFileResponse = await Client.GetAsync(dbExample.ImageUrl, TestContext.Current.CancellationToken);
        getFileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getFileResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
        
        var downloadedBytes = await getFileResponse.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        downloadedBytes.Length.Should().BeGreaterThan(0);
    }
}


