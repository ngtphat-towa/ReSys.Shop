using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Features.Testing.Examples.CreateExample;

namespace ReSys.Api.IntegrationTests.Features.Testing.Examples;

[Collection("Shared Database")]
public class CreateExampleTests : BaseIntegrationTest
{
    public CreateExampleTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "POST /api/testing/examples: Should create a new example")]
    public async Task Post_WithValidRequest_CreatesExample()
    {
        var request = new CreateExample.Request
        {
            Name = "NewExample",
            Description = "Desc",
            Price = 10,
            Status = ExampleStatus.Active,
            HexColor = "#FF5733"
        };

        var response = await Client.PostAsync("/api/testing/examples", new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(content, JsonSettings);
        var example = apiResponse!.Data;

        example!.Name.Should().Be("NewExample");
        example.Id.Should().NotBeEmpty();
        example.Status.Should().Be(ExampleStatus.Active);
        example.HexColor.Should().Be("#FF5733");
    }

    [Fact(DisplayName = "POST /api/testing/examples: Should return Created with correct Location header")]
    public async Task Post_ReturnsCorrectLocation()
    {
        var request = new CreateExample.Request { Name = "LocationTest", Description = "D", Price = 1 };

        var response = await Client.PostAsync("/api/testing/examples", new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleDetail>>(content, JsonSettings);
        var example = apiResponse!.Data;

        response.Headers.Location!.ToString().Should().Be($"/api/testing/examples/{example!.Id}");
    }

    [Fact(DisplayName = "POST /api/testing/examples: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        var name = "DuplicateTest";
        var r1 = new CreateExample.Request { Name = name, Description = "D", Price = 1 };
        await Client.PostAsync("/api/testing/examples", new StringContent(JsonConvert.SerializeObject(r1, JsonSettings), Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

        var r2 = new CreateExample.Request { Name = name, Description = "D", Price = 2 };
        var response = await Client.PostAsync("/api/testing/examples", new StringContent(JsonConvert.SerializeObject(r2, JsonSettings), Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "POST /api/testing/examples: Should return BadRequest for invalid input (e.g. empty name)")]
    public async Task Post_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreateExample.Request { Name = "", Description = "D", Price = -1 };

        var response = await Client.PostAsync("/api/testing/examples", new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
