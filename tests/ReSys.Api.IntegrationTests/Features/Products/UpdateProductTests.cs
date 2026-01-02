using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.UpdateProduct;
using ReSys.Core.Features.Products.Common;
using System.Net;
using System.Net.Http.Json;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class UpdateProductTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "PUT /api/products/{id}: Should successfully update product when request is valid")]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var productId = Guid.NewGuid();
        await SeedProductAsync("Old", 10, productId);
        var request = new UpdateProduct.Request { Name = "New", Description = "Updated", Price = 20 };

        var response = await Client.PutAsJsonAsync($"/api/products/{productId}", request, JsonOptions);

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDetail>(JsonOptions);
        product!.Name.Should().Be("New");
    }

    [Fact(DisplayName = "PUT /api/products/{id}: Should return 409 Conflict when updating name to an existing one")]
    public async Task Update_DuplicateName_ReturnsConflict()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        await SeedProductAsync("Product1", 10, p1);
        await SeedProductAsync("Product2", 20, p2);
        var request = new UpdateProduct.Request { Name = "Product1", Description = "D", Price = 30 };

        var response = await Client.PutAsJsonAsync($"/api/products/{p2}", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "PUT /api/products/{id}: Should return 404 Not Found when product does not exist")]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var request = new UpdateProduct.Request { Name = "Valid", Description = "D", Price = 10 };

        var response = await Client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedProductAsync(string name, decimal price, Guid id)
    {
        Context.Set<Product>().Add(new Product { Id = id, Name = name, Description = "D", Price = price });
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}