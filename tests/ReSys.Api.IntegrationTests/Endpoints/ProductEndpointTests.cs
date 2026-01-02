using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Api.Features.Products;
using ReSys.Core.Entities;
using ReSys.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReSys.Api.IntegrationTests.Endpoints;

public class ProductEndpointTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory = factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProducts_ShouldReturnEmpty_WhenDatabaseIsFresh()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions);
        products.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnProducts_WhenDatabaseHasData()
    {
        // Arrange
        var seededProducts = await SeedProductsAsync(2);
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions);
        
        products.Should().NotBeNull();
        products.Should().HaveCount(2);
        products.Should().BeEquivalentTo(seededProducts, options => 
            options.Excluding(p => p.Embedding).Excluding(p => p.CreatedAt));
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var seededProducts = await SeedProductsAsync(1);
        var expectedProduct = seededProducts.First();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/products/{expectedProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);

        product.Should().NotBeNull();
        product.Should().BeEquivalentTo(expectedProduct, options =>
            options.Excluding(p => p.Embedding).Excluding(p => p.CreatedAt));
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("New Product", "New Description", 99.99m);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request, _jsonOptions);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
        createdProduct.Should().NotBeNull();
        createdProduct!.Id.Should().NotBeEmpty();
        createdProduct.Name.Should().Be(request.Name);
        createdProduct.Description.Should().Be(request.Description);
        createdProduct.Price.Should().Be(request.Price);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnUpdatedProduct_WhenExists()
    {
        // Arrange
        var seededProducts = await SeedProductsAsync(1);
        var existingProduct = seededProducts.First();
        var client = _factory.CreateClient();
        var request = new UpdateProductRequest("Updated Name", "Updated Description", 150.00m);

        // Act
        var response = await client.PutAsJsonAsync($"/api/products/{existingProduct.Id}", request, _jsonOptions);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedProduct = await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);

        updatedProduct.Should().NotBeNull();
        updatedProduct!.Id.Should().Be(existingProduct.Id);
        updatedProduct.Name.Should().Be(request.Name);
        updatedProduct.Description.Should().Be(request.Description);
        updatedProduct.Price.Should().Be(request.Price);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenExists()
    {
        // Arrange
        var seededProducts = await SeedProductsAsync(1);
        var existingProduct = seededProducts.First();
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/products/{existingProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify Deletion
        var getResponse = await client.GetAsync($"/api/products/{existingProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<List<Product>> SeedProductsAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Test Product {i}",
                Description = $"Description for product {i}",
                Price = 10.99m * i,
                ImageUrl = $"https://example.com/image-{i}.jpg",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        return products;
    }
}
