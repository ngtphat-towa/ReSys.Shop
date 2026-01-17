using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;
using ReSys.Core.UnitTests.TestInfrastructure;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyQueryTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "BuildTaxonTree: Should handle Breadcrumbs and ActivePath in 32-node hierarchy")]
    public async Task BuildTree_MassiveHierarchy_ShouldReturnCorrectMetadata()
    {
        /*
         * VISUAL STRUCTURE (32 Nodes, 6 Levels):
         * Fashion (Root)
         * ├── Men (L1)
         * │   ├── Men Clothing (L2)
         * │   │   ├── Men Tops (L3)
         * │   │   │   └── T-Shirts (L4)
         * │   │   │       └── Oversized (L5 - Target Focus)
         * ...
         */

        // Arrange
        var taxonomy = Taxonomy.Create("QueryFashion").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var rootId = (await fixture.Context.Set<Taxon>().FirstAsync(t => t.TaxonomyId == taxonomy.Id && t.ParentId == null, TestContext.Current.CancellationToken)).Id;

        // Layer 1
        var men = Taxon.Create(taxonomy.Id, "Men", rootId).Value;
        var women = Taxon.Create(taxonomy.Id, "Women", rootId).Value;
        fixture.Context.Set<Taxon>().AddRange(men, women);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Layer 2
        var mCloth = Taxon.Create(taxonomy.Id, "Men Clothing", men.Id).Value;
        var mShoes = Taxon.Create(taxonomy.Id, "Men Shoes", men.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(mCloth, mShoes);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Layer 3
        var mTops = Taxon.Create(taxonomy.Id, "Men Tops", mCloth.Id).Value;
        var mBots = Taxon.Create(taxonomy.Id, "Men Bottoms", mCloth.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(mTops, mBots);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Layer 4
        var tshirts = Taxon.Create(taxonomy.Id, "T-Shirts", mTops.Id).Value;
        fixture.Context.Set<Taxon>().Add(tshirts);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Layer 5
        var oversized = Taxon.Create(taxonomy.Id, "Oversized", tshirts.Id).Value;
        fixture.Context.Set<Taxon>().Add(oversized);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Act: Focus on Depth 5 node
        var options = new TaxonQueryOptions
        {
            TaxonomyId = [taxonomy.Id],
            FocusedTaxonId = oversized.Id
        };
        var response = await _service.BuildTaxonTreeAsync(options, TestContext.Current.CancellationToken);

        // Assert
        response.FocusedNode.Should().NotBeNull();
        response.FocusedNode!.Name.Should().Be("Oversized");

        // Breadcrumbs: Root -> Men -> Men Clothing -> Men Tops -> T-Shirts -> Oversized (6 levels)
        response.Breadcrumbs.Should().HaveCount(6);
        response.Breadcrumbs.Select(b => b.Name).Should().ContainInOrder(["QueryFashion", "Men", "Men Clothing", "Men Tops", "T-Shirts", "Oversized"]);

        // Verify Active Path Flags
        var treeRoot = response.Tree.First();
        treeRoot.IsInActivePath.Should().BeTrue();
        treeRoot.IsExpanded.Should().BeTrue();

        var nodeMen = treeRoot.Children.First(c => c.Name == "Men");
        nodeMen.IsInActivePath.Should().BeTrue();

        var nodeWomen = treeRoot.Children.First(c => c.Name == "Women");
        nodeWomen.IsInActivePath.Should().BeFalse();
    }

    [Fact(DisplayName = "GetFlatTaxons: Should apply complex sorting and pagination on massive data")]
    public async Task GetFlat_MassiveData_PagingWorks()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("PagingFashion").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var rootId = (await fixture.Context.Set<Taxon>().FirstAsync(t => t.TaxonomyId == taxonomy.Id && t.ParentId == null, TestContext.Current.CancellationToken)).Id;

        // Generate 20 sub-nodes at Level 1 for easy paging
        for (int i = 1; i <= 20; i++)
        {
            fixture.Context.Set<Taxon>().Add(Taxon.Create(taxonomy.Id, $"Item-{i:D2}", rootId).Value);
        }
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Act: Sort by Name DESC, Page 2, Size 5
        var options = new TaxonQueryOptions
        {
            TaxonomyId = [taxonomy.Id],
            Sort = "Name desc",
            Page = 2,
            PageSize = 5
        };
        var result = await _service.GetFlatTaxonsAsync(options, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(21); // 20 items + 1 root
        // Page 1 DESC: Root(PagingFashion), Item-20, Item-19, Item-18, Item-17
        // Page 2 DESC: Item-16, Item-15, Item-14, Item-13, Item-12
        result.Items.Select(i => i.Name).Should().ContainInOrder(["Item-16", "Item-15", "Item-14", "Item-13", "Item-12"]);
    }

    [Fact(DisplayName = "BuildTaxonTree: Should respect MaxDepth in deep hierarchy")]
    public async Task BuildTree_DeepMaxDepth_Works()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("DepthLimit").Value;
        var l1 = taxonomy.AddTaxon("L1").Value;
        var l2 = taxonomy.AddTaxon("L2", l1.Id).Value;
        var l3 = taxonomy.AddTaxon("L3", l2.Id).Value;

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Act: MaxDepth = 2 (Root, L1, L2)
        var options = new TaxonQueryOptions { TaxonomyId = [taxonomy.Id], MaxDepth = 2 };
        var response = await _service.BuildTaxonTreeAsync(options, TestContext.Current.CancellationToken);

        // Assert
        var root = response.Tree.First();
        var nodeL1 = root.Children.First();
        var nodeL2 = nodeL1.Children.First();
        nodeL2.Children.Should().BeEmpty(); // L3 is depth 3, filtered out
    }

    [Fact(DisplayName = "GetFlatTaxons: Should filter by IncludeLeavesOnly")]
    public async Task GetFlat_LeavesOnly_Works()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("LeafQuery").Value;
        var parent = taxonomy.AddTaxon("Parent").Value;
        taxonomy.AddTaxon("LeafNode", parent.Id);

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Act
        var options = new TaxonQueryOptions { TaxonomyId = [taxonomy.Id], IncludeLeavesOnly = true };
        var result = await _service.GetFlatTaxonsAsync(options, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("LeafNode");
    }
}