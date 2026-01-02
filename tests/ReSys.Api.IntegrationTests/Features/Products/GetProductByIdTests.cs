using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using System.Net;
using System.Net.Http.Json;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class GetProductByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "GET /api/products/{id}: Should return product details when it exists")]
    public async Task Get_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        await SeedProductAsync(id, "Test");

        var response = await Client.GetAsync($"/api/products/{id}");

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDetail>(JsonOptions);
        product!.Id.Should().Be(id);
    }

    [Fact(DisplayName = "GET /api/products/{id}: Should return 404 Not Found when product does not exist")]
    public async Task Get_NonExistent_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedProductAsync(Guid id, string name)
    {
        Context.Set<Product>().Add(new Product { Id = id, Name = name, Description = "D", Price = 1 });
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}