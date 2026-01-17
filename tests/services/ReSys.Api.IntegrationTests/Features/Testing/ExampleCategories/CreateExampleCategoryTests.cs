using System.Net;
using System.Net.Http.Json;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Testing.ExampleCategories;

[Collection("Shared Database")]
public class CreateExampleCategoryTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/testing/example-categories: Should create a new category")]
    public async Task Post_WithValidRequest_CreatesCategory()
    {
        var request = new CreateExampleCategory.Request
        {
            Name = $"NewCat_{Guid.NewGuid()}",
            Description = "Test Description"
        };

        var response = await Client.PostAsJsonAsync("/api/testing/example-categories", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(content, JsonSettings);
        var category = apiResponse!.Data;

        category!.Name.Should().Be(request.Name);
        category.Id.Should().NotBeEmpty();

        response.Headers.Location!.ToString().Should().Be($"/api/testing/example-categories/{category.Id}");
    }

    [Fact(DisplayName = "POST /api/testing/example-categories: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        var name = $"Dup_{Guid.NewGuid()}";
        var request = new CreateExampleCategory.Request { Name = name };

        await Client.PostAsJsonAsync("/api/testing/example-categories", request, TestContext.Current.CancellationToken);
        var response = await Client.PostAsJsonAsync("/api/testing/example-categories", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "POST /api/testing/example-categories: Should return BadRequest for invalid input")]
    public async Task Post_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreateExampleCategory.Request { Name = "" }; // Required

        var response = await Client.PostAsJsonAsync("/api/testing/example-categories", request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
