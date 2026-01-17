using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes.Values;

[Collection("Shared Database")]
public class DeleteOptionValueTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "DELETE /api/catalog/option-types/{optionTypeId}/values/{id}: Should delete option value")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        // Arrange
        var optionType = await SeedOptionTypeAsync("DeleteTest");
        var value = await SeedOptionValueAsync(optionType.Id, "Gone", "G");

        // Act
        var response = await Client.DeleteAsync($"/api/catalog/option-types/{optionType.Id}/values/{value.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<OptionValueModel> SeedOptionValueAsync(Guid optionTypeId, string name, string presentation)
    {
        var request = new CreateOptionValue.Request { Name = name, Presentation = presentation };
        var response = await Client.PostAsync($"/api/catalog/option-types/{optionTypeId}/values",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonConvert.DeserializeObject<ApiResponse<OptionValueModel>>(content, JsonSettings)!.Data!;
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
