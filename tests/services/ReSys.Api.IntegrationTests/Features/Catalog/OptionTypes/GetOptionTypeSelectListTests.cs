using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;
using ReSys.Shared.Models.Wrappers;
using ReSys.Shared.Models.Pages;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class GetOptionTypeSelectListTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/catalog/option-types/select-list: Should return list")]
    public async Task Get_SelectList_ReturnsData()
    {
        // Arrange
        await SeedOptionTypeAsync($"Select_{Guid.NewGuid()}");

        // Act
        var response = await Client.GetAsync("/api/catalog/option-types/select-list", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OptionTypeSelectListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().NotBeEmpty();
    }

    private async Task SeedOptionTypeAsync(string name)
    {
        var request = new CreateOptionType.Request { Name = name, Presentation = name };
        await Client.PostAsync("/api/catalog/option-types", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
    }
}
