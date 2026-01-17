using ReSys.Core.Features.Testing.Examples.Common;
using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class GetExampleByIdTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/testing/examples/{id}: Should return Example details")]
    public async Task GetById_ExistingExample_ReturnsExample()
    {
        var ExampleId = await SeedExampleAsync("ByIdTest", 50);

        var response = await Client.GetAsync($"/api/testing/examples/{ExampleId}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(content, JsonSettings);
        var Example = apiResponse!.Data;
        
        Example!.Id.Should().Be(ExampleId);
        Example.Name.Should().Be("ByIdTest");
        Example.Status.Should().Be(ExampleStatus.Draft);
        Example.HexColor.Should().Be("#000000");
    }

    [Fact(DisplayName = "GET /api/testing/examples/{id}: Should return NotFound for non-existent id")]
    public async Task GetById_NonExistentExample_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/testing/examples/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedExampleAsync(string name, decimal price)
    {
        var id = Guid.NewGuid();
        Context.Set<Example>().Add(new Example
        {
            Id = id,
            Name = name,
            Description = "D",
            Price = price,
            CreatedAt = DateTimeOffset.UtcNow,
            ImageUrl = "",
            Status = ExampleStatus.Draft,
            HexColor = "#000000"
        });
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return id;
    }
}


