using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Common.Models;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using Xunit;

namespace ReSys.Api.IntegrationTests.Features.Testing.ExampleCategories;

[Collection("Shared Database")]
public class GetExampleCategoriesTests : BaseIntegrationTest
{
    public GetExampleCategoriesTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
        : base(factory, output)
    {
    }

    [Fact(DisplayName = "GET /api/testing/example-categories: Should support filtering and pagination")]
    public async Task Get_WithFilters_ReturnsPagedList()
    {
        // Arrange
        var baseName = $"List_{Guid.NewGuid()}";
        await Client.PostAsJsonAsync("/api/testing/example-categories", new CreateExampleCategory.Request { Name = $"{baseName}_1" }, TestContext.Current.CancellationToken);
        await Client.PostAsJsonAsync("/api/testing/example-categories", new CreateExampleCategory.Request { Name = $"{baseName}_2" }, TestContext.Current.CancellationToken);

        // Act
        var response = await Client.GetAsync($"/api/testing/example-categories?filter=Name*{baseName}&sort=Name desc", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ExampleCategoryListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        apiResponse.Data![0].Name.Should().Be($"{baseName}_2");
        apiResponse.Data[1].Name.Should().Be($"{baseName}_1");
        apiResponse.Meta.Should().NotBeNull();
        apiResponse.Meta!.TotalCount.Should().Be(2);
    }
}
