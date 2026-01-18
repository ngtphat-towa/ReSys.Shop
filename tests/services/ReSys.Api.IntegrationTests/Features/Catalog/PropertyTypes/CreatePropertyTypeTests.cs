using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;
using ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.PropertyTypes;

[Collection("Shared Database")]
public class CreatePropertyTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/catalog/property-types: Should create a new property type")]
    public async Task Post_WithValidRequest_CreatesPropertyType()
    {
        // Arrange
        var request = new CreatePropertyType.Request 
        { 
            Name = $"Property_{Guid.NewGuid()}", 
            Presentation = "Product Property",
            Kind = PropertyKind.String
        };

        // Act
        var response = await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.Presentation.Should().Be("Product Property");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "POST /api/catalog/property-types: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var name = $"DuplicateProp_{Guid.NewGuid()}";
        var request = new CreatePropertyType.Request { Name = name, Presentation = "P", Kind = PropertyKind.String };
        await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Act
        var response = await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(PropertyTypeErrors.DuplicateName.Code.ToSnakeCase());
    }

    [Fact(DisplayName = "POST /api/catalog/property-types: Should return BadRequest for invalid input")]
    public async Task Post_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreatePropertyType.Request { Name = "", Presentation = "P", Kind = PropertyKind.String };

        // Act
        var response = await Client.PostAsync("/api/catalog/property-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PropertyTypeDetail>>(content, JsonSettings);
        apiResponse!.Errors.Should().ContainKey("Request.Name");
    }
}
