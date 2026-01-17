using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class CreateOptionTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/catalog/option-types: Should create a new option type")]
    public async Task Post_WithValidRequest_CreatesOptionType()
    {
        // Arrange
        var request = new CreateOptionType.Request 
        { 
            Name = $"Size_{Guid.NewGuid()}", 
            Presentation = "Product Size" 
        };

        // Act
        var response = await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionTypeDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.Presentation.Should().Be("Product Size");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "POST /api/catalog/option-types: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var name = $"Duplicate_{Guid.NewGuid()}";
        var request = new CreateOptionType.Request { Name = name, Presentation = "P" };
        await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Act
        var response = await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
