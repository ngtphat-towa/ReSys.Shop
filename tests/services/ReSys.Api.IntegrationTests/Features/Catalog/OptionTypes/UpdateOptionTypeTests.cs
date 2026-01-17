using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Catalog.OptionTypes.UpdateOptionType;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class UpdateOptionTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "PUT /api/catalog/option-types/{id}: Should update option type")]
    public async Task Put_WithValidRequest_UpdatesOptionType()
    {
        // Arrange
        var created = await SeedOptionTypeAsync($"Old_{Guid.NewGuid()}");
        var request = new UpdateOptionType.Request { Name = $"New_{Guid.NewGuid()}", Presentation = "New Presentation" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/option-types/{created.Id}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionTypeDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.Presentation.Should().Be("New Presentation");
    }

    private async Task<OptionTypeDetail> SeedOptionTypeAsync(string name)
    {
        var request = new CreateOptionType.Request { Name = name, Presentation = name };
        var response = await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonConvert.DeserializeObject<ApiResponse<OptionTypeDetail>>(content, JsonSettings)!.Data!;
    }
}
