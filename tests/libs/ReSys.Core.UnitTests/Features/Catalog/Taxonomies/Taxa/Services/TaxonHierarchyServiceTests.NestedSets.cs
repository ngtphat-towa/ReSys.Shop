using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

using Microsoft.Extensions.Logging.Abstractions;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyNestedSetsTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "RebuildNestedSets: Should calculate Lft and Rgt correctly")]
    public async Task RebuildNestedSets_ShouldSetValuesCorrectly()
    {
        // Arrange
        // Root (1, 6)
        //  - Child A (2, 3)
        //  - Child B (4, 5)
        var taxonomy = Taxonomy.Create("NestedSetsTest").Value;
        var root = taxonomy.RootTaxon!;
        var childA = taxonomy.AddTaxon("A").Value;
        var childB = taxonomy.AddTaxon("B").Value;

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.RebuildNestedSetsAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        var taxons = fixture.Context.Set<Taxon>().Where(t => t.TaxonomyId == taxonomy.Id).ToList();
        var dbRoot = taxons.First(t => t.Id == root.Id);
        var dbA = taxons.First(t => t.Id == childA.Id);
        var dbB = taxons.First(t => t.Id == childB.Id);

        dbRoot.Lft.Should().Be(1);
        dbRoot.Rgt.Should().Be(6);
        dbRoot.Depth.Should().Be(0);

        dbA.Lft.Should().Be(2);
        dbA.Rgt.Should().Be(3);
        dbA.Depth.Should().Be(1);

        dbB.Lft.Should().Be(4);
        dbB.Rgt.Should().Be(5);
        dbB.Depth.Should().Be(1);
    }
}
