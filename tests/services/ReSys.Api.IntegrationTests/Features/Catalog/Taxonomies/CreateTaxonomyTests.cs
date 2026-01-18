using System.Net;
using System.Text;

using Newtonsoft.Json;

using ReSys.Api.IntegrationTests.TestInfrastructure;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Shared.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.IntegrationTests.Features.Catalog.Taxonomies;

[Collection("Shared Database")]
public class CreateTaxonomyTests(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : BaseIntegrationTest(factory, output)
{
    [Fact(DisplayName = "POST /api/catalog/taxonomies: Should create a new taxonomy")]
    public async Task Post_WithValidRequest_CreatesTaxonomy()
    {
        // Arrange
        var request = new CreateTaxonomy.Request
        {
            Name = $"Taxonomy_{Guid.NewGuid()}",
            Presentation = "Product Categories"
        };

        // Act
        var response = await Client.PostAsync("/api/catalog/taxonomies",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TaxonomyDetail>>(content, JsonSettings);

        apiResponse!.Data!.Name.Should().Be(request.Name);
        apiResponse.Data.Presentation.Should().Be("Product Categories");
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "POST /api/catalog/taxonomies: Should return Conflict for duplicate name")]
    public async Task Post_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var name = $"DuplicateTax_{Guid.NewGuid()}";
        var request = new CreateTaxonomy.Request { Name = name, Presentation = "P" };
        await Client.PostAsync("/api/catalog/taxonomies",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Act
        var response = await Client.PostAsync("/api/catalog/taxonomies",
            new StringContent(JsonConvert.SerializeObject(request, JsonSettings), Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TaxonomyDetail>>(content, JsonSettings);
        apiResponse!.ErrorCode.Should().Be(TaxonomyErrors.DuplicateName.Code.ToSnakeCase());
    }
}
