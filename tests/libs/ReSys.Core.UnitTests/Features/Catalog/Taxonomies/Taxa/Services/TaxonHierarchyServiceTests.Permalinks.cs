using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

using Microsoft.Extensions.Logging.Abstractions;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyPermalinksTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "RegeneratePermalinks: Should build hierarchical slugs and names")]
    public async Task RegeneratePermalinks_ShouldSyncPaths()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("PermalinksTest").Value;
        var root = taxonomy.RootTaxon!;
        var child = taxonomy.AddTaxon("Electronics").Value;
        var subChild = taxonomy.AddTaxon("Phones", child.Id).Value;

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.RegeneratePermalinksAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        var dbChild = fixture.Context.Set<Taxon>().First(t => t.Id == child.Id);
        var dbSubChild = fixture.Context.Set<Taxon>().First(t => t.Id == subChild.Id);

        dbChild.Permalink.Should().Be("permalinkstest/electronics");
        dbSubChild.Permalink.Should().Be("permalinkstest/electronics/phones");
        dbSubChild.PrettyName.Should().Be("PermalinksTest -> Electronics -> Phones");
    }
}