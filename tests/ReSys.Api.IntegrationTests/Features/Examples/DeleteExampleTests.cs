using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain;


using System.Net;

namespace ReSys.Api.IntegrationTests.Features.Examples;

[Collection("Shared Database")]
public class DeleteExampleTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "DELETE /api/examples/{id}: Should return 204 No Content and delete the Example")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        await SeedExampleAsync(id);

        var response = await Client.DeleteAsync($"/api/examples/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/examples/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/examples/{id}: Should return 404 Not Found when Example does not exist")]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await Client.DeleteAsync($"/api/examples/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedExampleAsync(Guid id)
    {
        Context.Set<Example>().Add(new Example { Id = id, Name = "To Delete", Description = "D", Price = 1 });
        await Context.SaveChangesAsync(CancellationToken.None);
    }
}