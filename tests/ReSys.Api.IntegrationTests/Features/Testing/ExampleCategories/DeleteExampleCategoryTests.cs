using System.Net;
using System.Net.Http.Json;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;

namespace ReSys.Api.IntegrationTests.Features.Testing.ExampleCategories;

[Collection("Shared Database")]
public class DeleteExampleCategoryTests : BaseIntegrationTest
{
    public DeleteExampleCategoryTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "DELETE /api/testing/example-categories/{id}: Should delete category")]
    public async Task Delete_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateExampleCategory.Request { Name = $"ToDelete_{Guid.NewGuid()}" };
        var createResponse = await Client.PostAsJsonAsync("/api/testing/example-categories", createRequest, TestContext.Current.CancellationToken);
        var created = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), JsonSettings);
        var id = created!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/testing/example-categories/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify gone
        var getResponse = await Client.GetAsync($"/api/testing/example-categories/{id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/testing/example-categories/{id}: Should return NotFound for non-existent id")]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await Client.DeleteAsync($"/api/testing/example-categories/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
