using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using ReSys.Core.Features.Testing.ExampleCategories.UpdateExampleCategory;
using Xunit;

namespace ReSys.Api.IntegrationTests.Features.Testing.ExampleCategories;

[Collection("Shared Database")]
public class UpdateExampleCategoryTests : BaseIntegrationTest
{
    public UpdateExampleCategoryTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "PUT /api/testing/example-categories/{id}: Should update category")]
    public async Task Put_WithValidRequest_UpdatesCategory()
    {
        // Arrange
        var createRequest = new CreateExampleCategory.Request { Name = $"ToUpdate_{Guid.NewGuid()}" };
        var createResponse = await Client.PostAsJsonAsync("/api/testing/example-categories", createRequest, TestContext.Current.CancellationToken);
        var created = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), JsonSettings);
        var id = created!.Data!.Id;

        var updateRequest = new UpdateExampleCategory.Request { Name = $"Updated_{Guid.NewGuid()}", Description = "New Desc" };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/testing/example-categories/{id}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(updateRequest.Name);
        apiResponse.Data.Description.Should().Be("New Desc");
    }

    [Fact(DisplayName = "PUT /api/testing/example-categories/{id}: Should return Conflict for duplicate name")]
    public async Task Put_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var name1 = $"Name1_{Guid.NewGuid()}";
        var name2 = $"Name2_{Guid.NewGuid()}";
        
        await Client.PostAsJsonAsync("/api/testing/example-categories", new CreateExampleCategory.Request { Name = name1 }, TestContext.Current.CancellationToken);
        var res2 = await Client.PostAsJsonAsync("/api/testing/example-categories", new CreateExampleCategory.Request { Name = name2 }, TestContext.Current.CancellationToken);
        var cat2 = JsonConvert.DeserializeObject<ApiResponse<ExampleCategoryDetail>>(await res2.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), JsonSettings);

        var updateRequest = new UpdateExampleCategory.Request { Name = name1 };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/testing/example-categories/{cat2!.Data!.Id}", updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
