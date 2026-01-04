using System.Net.Http.Headers;
using System.Text;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.Common;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Models;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Storage;

namespace ReSys.Api.IntegrationTests.Features.Examples;

[Collection("Shared Database")]
public class UpdateExampleImageTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/examples/{id}/image: Should upload image and update Example URL")]
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
            ImageUrl = ""
        });
        await Context.SaveChangesAsync(CancellationToken.None);

        using var content = new MultipartFormDataContent();
        
        // Find real PNG asset
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
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

        var imageBytes = await File.ReadAllBytesAsync(assetPath);
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        
        content.Add(fileContent, "image", "test-image.png");

        // Act
        var response = await Client.PostAsync($"/api/examples/{ExampleId}/image", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Request Failed: {response.StatusCode} \nContent: {responseString}");
        }

        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(responseString, JsonSettings);
        var result = apiResponse!.Data;

        result!.ImageUrl.Should().Contain(".png");
        result.Id.Should().Be(ExampleId);

        // Verify DB update
        ((DbContext)Context).ChangeTracker.Clear();
        var dbExample = await Context.Set<Example>().FindAsync(ExampleId);

        dbExample.Should().NotBeNull();

        dbExample!.ImageUrl!.Should().Contain(".png");

        // Verify file storage
        var fileName = dbExample.ImageUrl!.Split('/').Last();
        
        using var scope = Factory.Services.CreateScope();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var existsResult = await fileService.FileExistsAsync(fileName);
        
        existsResult.IsError.Should().BeFalse();
        existsResult.Value.Should().BeTrue("because the file was successfully saved");
    }
}
