using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Catalog.OptionTypes.UpdateOptionType;
using ReSys.Shared.Extensions;
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

    [Fact(DisplayName = "PUT /api/catalog/option-types/{id}: Should return Conflict when updating to existing name")]
    public async Task Put_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var ot1 = await SeedOptionTypeAsync($"Conflict1_{Guid.NewGuid()}");
        var name2 = $"Conflict2_{Guid.NewGuid()}";
        await SeedOptionTypeAsync(name2);

        var request = new UpdateOptionType.Request { Name = name2, Presentation = "P" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/option-types/{ot1.Id}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<OptionTypeDetail>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(OptionTypeErrors.DuplicateName.Code.ToSnakeCase());
    }

    [Fact(DisplayName = "PUT /api/catalog/option-types/{id}: Should return NotFound for non-existent ID")]
    public async Task Put_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateOptionType.Request { Name = "Valid", Presentation = "P" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/option-types/{Guid.NewGuid()}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
