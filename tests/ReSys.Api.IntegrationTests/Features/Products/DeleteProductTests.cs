using FluentAssertions;
using ReSys.Core.Entities;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using System.Net;

namespace ReSys.Api.IntegrationTests.Features.Products;

[Collection("Shared Database")]
public class DeleteProductTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "DELETE /api/products/{id}: Should return 204 No Content and delete the product")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        await SeedProductAsync(id);

        var response = await Client.DeleteAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/products/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/products/{id}: Should return 404 Not Found when product does not exist")]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await Client.DeleteAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedProductAsync(Guid id)
    {
        Context.Set<Product>().Add(new Product { Id = id, Name = "To Delete", Description = "D", Price = 1 });
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}