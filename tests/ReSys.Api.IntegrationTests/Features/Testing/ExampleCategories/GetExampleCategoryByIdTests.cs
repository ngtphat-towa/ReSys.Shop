using System.Net;
using System.Net.Http.Json;


using Newtonsoft.Json;


using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;

namespace ReSys.Api.IntegrationTests.Features.Testing.ExampleCategories;

[Collection("Shared Database")]
public class GetExampleCategoryByIdTests : BaseIntegrationTest
{
    public GetExampleCategoryByIdTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "GET /api/testing/example-categories/{id}: Should return category details")]
    public async Task Get_ExistingCategory_ReturnsDetails()
    {
        // Arrange
        var request = new CreateExampleCategory.Request { Name = $"GetById_{Guid.NewGuid()}" };
        var createResponse = await Client.PostAsJsonAsync("/api/testing/example-categories", request, TestContext.Current.CancellationToken);
        var createContent = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var created = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(createContent, JsonSettings);
        var id = created!.Data!.Id;

        // Act
        var response = await Client.GetAsync($"/api/testing/example-categories/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(content, JsonSettings);
        apiResponse!.Data!.Id.Should().Be(id);
        apiResponse.Data.Name.Should().Be(request.Name);
    }

    [Fact(DisplayName = "GET /api/testing/example-categories/{id}: Should return NotFound for non-existent id")]
    public async Task Get_NonExistent_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/testing/example-categories/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
