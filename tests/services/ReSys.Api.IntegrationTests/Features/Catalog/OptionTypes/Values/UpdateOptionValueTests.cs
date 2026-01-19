using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValue;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes;

[Collection("Shared Database")]
public class UpdateOptionValueTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "PUT /api/catalog/option-types/{optionTypeId}/values/{id}: Should update option value")]
    public async Task Put_WithValidRequest_UpdatesOptionValue()
    {
        // Arrange
        var optionType = await SeedOptionTypeAsync("Material");
        var value = await SeedOptionValueAsync(optionType.Id, "OldName", "OldPres");
        var request = new UpdateOptionValue.Request { Name = "NewName", Presentation = "NewPres" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/option-types/{optionType.Id}/values/{value.Id}",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionValueModel>>(content, JsonSettings);

        apiResponse!.Data!.Name.Should().Be("NewName");
        apiResponse.Data.Presentation.Should().Be("NewPres");
    }

    [Fact(DisplayName = "PUT /api/catalog/option-types/{optionTypeId}/values/{id}: Should return Conflict for duplicate name")]
    public async Task Put_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var optionType = await SeedOptionTypeAsync($"ValueConflict_{Guid.NewGuid()}");
        await SeedOptionValueAsync(optionType.Id, "Exist", "Exist");
        var value = await SeedOptionValueAsync(optionType.Id, "Old", "Old");
        
        var request = new UpdateOptionValue.Request { Name = "Exist", Presentation = "P" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/option-types/{optionType.Id}/values/{value.Id}",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionValueModel>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(OptionValueErrors.NameAlreadyExists("Exist").Code.ToSnakeCase());
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
