using System.Net;
using Newtonsoft.Json;
using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Features.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Shared.Models.Wrappers;
using System.Text;

namespace ReSys.Api.IntegrationTests.Features.Catalog.Taxonomies;

[Collection("Shared Database")]
public class GetTaxonomiesPagedListTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "GET /api/catalog/taxonomies: Should return paged list of taxonomies")]
    public async Task Get_WithPagination_ReturnsCorrectSlice()
    {
        // Arrange
        var unique = $"PagedTax_{Guid.NewGuid()}";
        await SeedTaxonomyAsync($"{unique}_1");
        await SeedTaxonomyAsync($"{unique}_2");

        // Act
        var response = await Client.GetAsync($"/api/catalog/taxonomies?search={unique}&page=1&page_size=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TaxonomyListItem>>>(content, JsonSettings);
        
        apiResponse!.Data.Should().HaveCount(2);
    }

    private async Task SeedTaxonomyAsync(string name)
    {
        var request = new CreateTaxonomy.Request { Name = name, Presentation = name };
        await Client.PostAsync("/api/catalog/taxonomies", 
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"), 
            TestContext.Current.CancellationToken);
    }
}
