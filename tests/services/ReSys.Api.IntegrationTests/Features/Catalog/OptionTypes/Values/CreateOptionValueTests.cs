using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes.Values;

[Collection("Shared Database")]
public class CreateOptionValueTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/catalog/option-types/{optionTypeId}/values: Should create a new option value")]
    public async Task Post_WithValidRequest_CreatesOptionValue()
    {
        // Arrange
        var optionType = await SeedOptionTypeAsync("Size");
        var request = new CreateOptionValue.Request { Name = "Small", Presentation = "S" };

        // Act
        var response = await Client.PostAsync($"/api/catalog/option-types/{optionType.Id}/values",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionValueModel>>(content, JsonSettings);

        apiResponse!.Data!.Name.Should().Be("Small");
        apiResponse.Data.Presentation.Should().Be("S");
    }

    [Fact(DisplayName = "POST /api/catalog/option-types/{optionTypeId}/values: Should return NotFound for missing OptionType")]
    public async Task Post_MissingOptionType_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateOptionValue.Request { Name = "Small", Presentation = "S" };

        // Act
        var response = await Client.PostAsync($"/api/catalog/option-types/{Guid.NewGuid()}/values",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionValueModel>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(OptionTypeErrors.NotFound(Guid.Empty).Code.ToSnakeCase());
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
