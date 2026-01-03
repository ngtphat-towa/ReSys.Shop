using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class GetProductByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "GET /api/products/{id}: Should return product details")]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        var productId = await SeedProductAsync("ByIdTest", 50);

        var response = await Client.GetAsync($"/api/products/{productId}");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductDetail>(content, JsonSettings);
        
        product!.Id.Should().Be(productId);
        product.Name.Should().Be("ByIdTest");
    }

    [Fact(DisplayName = "GET /api/products/{id}: Should return NotFound for non-existent id")]
    public async Task GetById_NonExistentProduct_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedProductAsync(string name, decimal price)
    {
        var id = Guid.NewGuid();
        Context.Set<Product>().Add(new Product { Id = id, Name = name, Description = "D", Price = price, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" });
        await Context.SaveChangesAsync(CancellationToken.None);
        return id;
    }
}
