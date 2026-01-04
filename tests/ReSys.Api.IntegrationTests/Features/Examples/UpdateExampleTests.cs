using ReSys.Core.Common.Models;
using ReSys.Core.Entities;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.Features.Examples.UpdateExample;
using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Examples;

[Collection("Shared Database")]
public class UpdateExampleTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "PUT /api/examples/{id}: Should update Example details")]
    public async Task Put_WithValidRequest_UpdatesExample()
    {
        var ExampleId = await SeedExampleAsync("UpdateTest", 10);
        var request = new UpdateExample.Request { Name = "UpdatedName", Description = "NewDesc", Price = 20 };

        var response = await Client.PutAsync($"/api/examples/{ExampleId}",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(content, JsonSettings);
        var Example = apiResponse!.Data;

        Example!.Name.Should().Be("UpdatedName");
        Example.Price.Should().Be(20);
    }

    [Fact(DisplayName = "PUT /api/examples/{id}: Should return Conflict if name taken by another Example")]
    public async Task Put_WithDuplicateName_ReturnsConflict()
    {
        var p1 = await SeedExampleAsync("P1", 10);
        var p2 = await SeedExampleAsync("P2", 20);
        var request = new UpdateExample.Request { Name = "P1", Description = "D", Price = 30 };

        var response = await Client.PutAsync($"/api/examples/{p2}",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "PUT /api/examples/{id}: Should return NotFound if Example missing")]
    public async Task Put_MissingExample_ReturnsNotFound()
    {
        var request = new UpdateExample.Request { Name = "N", Description = "D", Price = 10 };
        var response = await Client.PutAsync($"/api/examples/{Guid.NewGuid()}",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedExampleAsync(string name, decimal price)
    {
        var id = Guid.NewGuid();
        Context.Set<Example>().Add(new Example { Id = id, Name = name, Description = "D", Price = price, CreatedAt = DateTimeOffset.UtcNow, ImageUrl = "" });
        await Context.SaveChangesAsync(CancellationToken.None);
        return id;
    }
}
