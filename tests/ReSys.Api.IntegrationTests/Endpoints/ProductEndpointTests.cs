using FluentAssertions;
using ReSys.Core.Entities;
using System.Net.Http.Json;

namespace ReSys.Api.IntegrationTests.Endpoints;

public class ProductEndpointTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public ProductEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnEmpty_WhenDatabaseIsFresh()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().BeEmpty();
    }
}
