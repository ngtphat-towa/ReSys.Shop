using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.PropertyTypes;

[Collection("Shared Database")]
public class DeletePropertyTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "DELETE /api/catalog/property-types/{id}: Should delete property type")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        // Arrange
        var created = await SeedPropertyTypeAsync($"DeleteProp_{Guid.NewGuid()}");

        // Act
        var response = await Client.DeleteAsync($"/api/catalog/property-types/{created.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/catalog/property-types/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<PropertyTypeDetail> SeedPropertyTypeAsync(string name)
    {
        var request = new CreatePropertyType.Request { Name = name, Presentation = name, Kind = PropertyKind.String };
        var response = await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings)!.Data!;
    }
}
