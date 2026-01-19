using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValuePositions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.OptionTypes.Values;

[Collection("Shared Database")]
public class UpdateOptionValuePositionsTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "PUT /api/catalog/option-types/{optionTypeId}/values/positions: Should update positions")]
    public async Task Put_WithValidRequest_UpdatesPositions()
    {
        // Arrange
        var unique = $"Pos_{Guid.NewGuid()}";
        var optionType = await SeedOptionTypeAsync(unique);
        var v1 = await SeedOptionValueAsync(optionType.Id, $"{unique}_V1", "1");
        var v2 = await SeedOptionValueAsync(optionType.Id, $"{unique}_V2", "2");

        var request = new UpdateOptionValuePositions.Request(new[]
        {
            new UpdateOptionValuePositions.ValuePosition(v1.Id, 10),
            new UpdateOptionValuePositions.ValuePosition(v2.Id, 5)
        });

                        // Act

                        // Clear tracker if the seeding used the same context/scope

                        if (Context is DbContext dbContext)

                        {

                            dbContext.ChangeTracker.Clear();

                        }

                

                        var response = await Client.PutAsync($"/api/catalog/option-types/{optionType.Id}/values/positions", 

                            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 

                            TestContext.Current.CancellationToken);

                

                        // Assert

                                if (response.StatusCode != HttpStatusCode.OK)

                                {

                                    var errorBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

                                    Output.WriteLine($"Error Response: {errorBody}");

                                }

                        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
