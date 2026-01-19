using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.Taxonomies;

[Collection("Shared Database")]
public class DeleteTaxonomyTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "DELETE /api/catalog/taxonomies/{id}: Should delete taxonomy")]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        // Arrange
        var created = await SeedTaxonomyAsync($"DeleteTax_{Guid.NewGuid()}");

        // Act
        var response = await Client.DeleteAsync($"/api/catalog/taxonomies/{created.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync($"/api/catalog/taxonomies/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
