using FluentAssertions;
using ReSys.Core.Features.Products.CreateProduct;
using ReSys.Core.Features.Products.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class CreateProductTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "POST /api/products: Should return 201 Created and product details for a valid unique request")]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateProduct.Request { Name = "Unique Pro", Description = "Desc", Price = 100 };

        var response = await Client.PostAsJsonAsync("/api/products", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDetail>(JsonOptions);
        product!.Name.Should().Be(request.Name);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "POST /api/products: Should accept camelCase JSON request body and still create the product")]
    public async Task Create_CamelCaseRequestBody_ReturnsCreated()
    {
        // Arrange
        // We use a raw string to ensure we are sending actual camelCase keys
        var camelCaseJson = new { name = "CamelCase Pro", description = "Testing camel case", price = 150.00m };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/products", camelCaseJson, options);

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDetail>(JsonOptions);
        product!.Name.Should().Be("CamelCase Pro");
        product.Price.Should().Be(150.00m);
    }

    [Fact(DisplayName = "POST /api/products: Should return 409 Conflict when name already exists")]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        var name = "Existing";
        await Client.PostAsJsonAsync("/api/products", new CreateProduct.Request { Name = name, Description = "D", Price = 1 }, JsonOptions);

        var response = await Client.PostAsJsonAsync("/api/products", new CreateProduct.Request { Name = name, Description = "D", Price = 2 }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "POST /api/products: Should return 400 Bad Request when validation fails (e.g. negative price)")]
    public async Task Create_InvalidPrice_ReturnsBadRequest()
    {
        var request = new CreateProduct.Request { Name = "Invalid", Description = "D", Price = -10 };

        var response = await Client.PostAsJsonAsync("/api/products", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}