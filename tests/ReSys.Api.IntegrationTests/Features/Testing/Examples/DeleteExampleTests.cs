using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain;
using Xunit;

using System.Net;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class DeleteExampleTests : BaseIntegrationTest
{
    public DeleteExampleTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "DELETE /api/testing/examples/{id}: Should return 204 No Content and delete the Example")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        await SeedExampleAsync(id);

        var response = await Client.DeleteAsync($"/api/testing/examples/{id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/testing/examples/{id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/testing/examples/{id}: Should return 404 Not Found when Example does not exist")]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await Client.DeleteAsync($"/api/testing/examples/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedExampleAsync(Guid id)
    {
        Context.Set<Example>().Add(new Example
        {
            Id = id,
            Name = "To Delete",
            Description = "D",
            Price = 1,
            ImageUrl = "",
            Status = ExampleStatus.Draft,
            HexColor = "#000000"
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}

