using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Features.Products.UpdateProduct;
using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class UpdateProductTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "PUT /api/products/{id}: Should update product details")]
    public async Task Put_WithValidRequest_UpdatesProduct()
    {
        var productId = await SeedProductAsync("UpdateTest", 10);
        var request = new UpdateProduct.Request { Name = "UpdatedName", Description = "NewDesc", Price = 20 };

        var response = await Client.PutAsync($"/api/products/{productId}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductDetail>(content, JsonSettings);
        
        product!.Name.Should().Be("UpdatedName");
        product.Price.Should().Be(20);
    }

    [Fact(DisplayName = "PUT /api/products/{id}: Should return Conflict if name taken by another product")]
    public async Task Put_WithDuplicateName_ReturnsConflict()
    {
        var p1 = await SeedProductAsync("P1", 10);
        var p2 = await SeedProductAsync("P2", 20);
        var request = new UpdateProduct.Request { Name = "P1", Description = "D", Price = 30 };

        var response = await Client.PutAsync($"/api/products/{p2}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "PUT /api/products/{id}: Should return NotFound if product missing")]
    public async Task Put_MissingProduct_ReturnsNotFound()
    {
        var request = new UpdateProduct.Request { Name = "N", Description = "D", Price = 10 };
        var response = await Client.PutAsync($"/api/products/{Guid.NewGuid()}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

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
