using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.UpdateTaxonomy;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.Taxonomies;

[Collection("Shared Database")]
public class UpdateTaxonomyTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "PUT /api/catalog/taxonomies/{id}: Should update taxonomy")]
    public async Task Put_WithValidRequest_UpdatesTaxonomy()
    {
        // Arrange
        var created = await SeedTaxonomyAsync($"OldTax_{Guid.NewGuid()}");
        var request = new UpdateTaxonomy.Request { Name = $"NewTax_{Guid.NewGuid()}", Presentation = "New Presentation" };

        // Act
        var response = await Client.PutAsync($"/api/catalog/taxonomies/{created.Id}", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TaxonomyDetail>>(content, JsonSettings);
        
        apiResponse!.Data!.Name.Should().Be(request.Name);
    }

    private async Task<TaxonomyDetail> SeedTaxonomyAsync(string name)
    {
        var request = new CreateTaxonomy.Request { Name = name, Presentation = name };
        var response = await Client.PostAsync("/api/catalog/taxonomies", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonConvert.DeserializeObject<ApiResponse<TaxonomyDetail>>(content, JsonSettings)!.Data!;
    }
}
