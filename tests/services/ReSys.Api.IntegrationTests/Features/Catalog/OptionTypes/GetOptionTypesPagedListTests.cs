using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Shared.Models.Wrappers;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class GetOptionTypesPagedListTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/catalog/option-types: Should return paged list of option types")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        // Arrange
        var unique = $"PagedList_{Guid.NewGuid()}";
        await SeedOptionTypeAsync($"{unique}_1");
        await SeedOptionTypeAsync($"{unique}_2");

        // Act
        var response = await Client.GetAsync($"/api/catalog/option-types?search={unique}&page=1&page_size=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OptionTypeListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
        apiResponse.Data.Should().Contain(x => x.Name == $"{unique}_1");
        apiResponse.Data.Should().Contain(x => x.Name == $"{unique}_2");
    }

    [Fact(DisplayName = "GET /api/catalog/option-types: Random search should return empty")]
    public async Task Get_WithRandomSearch_ReturnsEmpty()
    {
        // Act
        var randomSearch = $"NoMatch_{Guid.NewGuid()}";
        var response = await Client.GetAsync($"/api/catalog/option-types?search={randomSearch}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OptionTypeListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().BeEmpty();
    }

    private async Task SeedOptionTypeAsync(string name)
    {
        var request = new CreateOptionType.Request { Name = name, Presentation = name };
        await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
    }
}
