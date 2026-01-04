using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Entities;
using ReSys.Core.Features.Examples.Common;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Models;

namespace ReSys.Api.IntegrationTests.Features.Examples;

[Collection("Shared Database")]
public class UpdateExampleImageTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/Examples/{id}/image: Should upload image and update Example URL")]
    public async Task UpdateImage_ValidFile_ReturnsUpdatedExample()
    {
        // Arrange
        var ExampleId = Guid.NewGuid();
        Context.Set<Example>().Add(new Example 
        { 
            Id = ExampleId, 
            Name = "ImageTest", 
            Description = "Desc", 
            Price = 10,
            CreatedAt = DateTimeOffset.UtcNow,
            ImageUrl = ""
        });
        await Context.SaveChangesAsync(CancellationToken.None);

        using var content = new MultipartFormDataContent();
        // Create dummy image content
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image content"));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        
        // Add to multipart form: name="image", filename="test-image.png"
        // 'image' matches IFormFile parameter name in endpoint
        content.Add(fileContent, "image", "test-image.png");

        // Act
        var response = await Client.PostAsync($"/api/Examples/{ExampleId}/image", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Request Failed: {response.StatusCode} \nContent: {responseString}");
        }

        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(responseString, JsonSettings);
        var result = apiResponse!.Data;

        result!.ImageUrl.Should().Contain("test-image.png");
        result.Id.Should().Be(ExampleId);

        // Verify DB update
        ((DbContext)Context).ChangeTracker.Clear();
        var dbExample = await Context.Set<Example>().FindAsync(ExampleId);
        dbExample!.ImageUrl.Should().Contain("test-image.png");

        // Verify file storage
        // ImageUrl format: /api/files/{fileName}
        var fileName = dbExample!.ImageUrl.Split('/').Last();
        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "test-storage", fileName);
        
        File.Exists(storagePath).Should().BeTrue($"File should exist at {storagePath}");
    }
}
