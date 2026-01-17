using System.Net;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using Newtonsoft.Json;
using System.Text;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class DeleteOptionTypeTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "DELETE /api/catalog/option-types/{id}: Should delete option type")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        // Arrange
        var created = await SeedOptionTypeAsync($"DeleteMe_{Guid.NewGuid()}");

        // Act
        var response = await Client.DeleteAsync($"/api/catalog/option-types/{created.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/catalog/option-types/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
