using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class UpdateProductImageTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/products/{id}/image: Should upload image and update product URL")]
    public async Task UpdateImage_ValidFile_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        Context.Set<Product>().Add(new Product 
        { 
            Id = productId, 
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
        var response = await Client.PostAsync($"/api/products/{productId}/image", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Request Failed: {response.StatusCode} \nContent: {responseString}");
        }

        var result = JsonConvert.DeserializeObject<ProductDetail>(responseString, JsonSettings);

        result!.ImageUrl.Should().Contain("test-image.png");
        result.Id.Should().Be(productId);

        // Verify DB update
        ((DbContext)Context).ChangeTracker.Clear();
        var dbProduct = await Context.Set<Product>().FindAsync(productId);
        dbProduct!.ImageUrl.Should().Contain("test-image.png");

        // Verify file storage
        // ImageUrl format: /api/files/{fileName}
        var fileName = dbProduct!.ImageUrl.Split('/').Last();
        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "test-storage", fileName);
        
        File.Exists(storagePath).Should().BeTrue($"File should exist at {storagePath}");
    }
}
