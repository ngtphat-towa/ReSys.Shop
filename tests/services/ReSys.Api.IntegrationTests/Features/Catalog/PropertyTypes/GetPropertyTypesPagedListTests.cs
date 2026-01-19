using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Shared.Models.Wrappers;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Catalog.PropertyTypes;

[Collection("Shared Database")]
public class GetPropertyTypesPagedListTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/catalog/property-types: Should return paged list of property types")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        // Arrange
        var unique = $"PagedProp_{Guid.NewGuid()}";
        await SeedPropertyTypeAsync($"{unique}_1");
        await SeedPropertyTypeAsync($"{unique}_2");

        // Act
        var response = await Client.GetAsync($"/api/catalog/property-types?search={unique}&page=1&page_size=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PropertyTypeListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
    }

    private async Task SeedPropertyTypeAsync(string name)
    {
        var request = new CreatePropertyType.Request { Name = name, Presentation = name, Kind = PropertyKind.String };
        await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
    }
}
