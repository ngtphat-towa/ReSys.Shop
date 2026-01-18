using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.PropertyTypes;

[Collection("Shared Database")]
public class UpdatePropertyTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "PUT /api/catalog/property-types/{id}: Should update property type")]
    public async Task Put_WithValidRequest_UpdatesPropertyType()
    {
        // Arrange
        var created = await SeedPropertyTypeAsync($"OldProp_{Guid.NewGuid()}");
        var request = new UpdatePropertyType.Request { Name = $"NewProp_{Guid.NewGuid()}", Presentation = "New Presentation", Kind = PropertyKind.Integer };

        // Act
        var response = await Client.PutAsync($"/api/catalog/property-types/{created.Id}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.Kind.Should().Be(PropertyKind.Integer);
    }

    [Fact(DisplayName = "PUT /api/catalog/property-types/{id}: Should return Conflict when updating to existing name")]
    public async Task Put_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var pt1 = await SeedPropertyTypeAsync($"PropConflict1_{Guid.NewGuid()}");
        var name2 = $"PropConflict2_{Guid.NewGuid()}";
        await SeedPropertyTypeAsync(name2);

        var request = new UpdatePropertyType.Request { Name = name2, Presentation = "P", Kind = PropertyKind.String };

        // Act
        var response = await Client.PutAsync($"/api/catalog/property-types/{pt1.Id}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(PropertyTypeErrors.DuplicateName.Code.ToSnakeCase());
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
