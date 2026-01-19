using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Shared.Models.Wrappers;
using ReSys.Shared.Models.Pages;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes.Values;

[Collection("Shared Database")]
public class GetOptionValuesTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/catalog/option-types/{optionTypeId}/values: Should return paged list of values")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        // Arrange
        var optionType = await SeedOptionTypeAsync("Color");
        var baseUrl = $"/api/catalog/option-types/{optionType.Id}/values";

        await SeedOptionValueAsync(optionType.Id, "Red", "R");
        await SeedOptionValueAsync(optionType.Id, "Blue", "B");

        // Act
        var response = await Client.GetAsync($"{baseUrl}?page=1&page_size=10", TestContext.Current.CancellationToken);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OptionValueModel>>>(content, JsonSettings);
                
                apiResponse!.Data.Should().HaveCount(2);
                apiResponse.Data.Should().Contain(x => x.Name == "Red");
                apiResponse.Data.Should().Contain(x => x.Name == "Blue");    }

    private async Task SeedOptionValueAsync(Guid optionTypeId, string name, string presentation)
    {
        var request = new CreateOptionValue.Request { Name = name, Presentation = presentation };
        await Client.PostAsync($"/api/catalog/option-types/{optionTypeId}/values",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);
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
