using Microsoft.Extensions.Logging.Abstractions;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyDeepTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "Fashion Massive Layered: 32 nodes, 6 levels, manual level-by-level creation")]
    public async Task Rebuild_FashionMassiveLayered_Success()
    {
        /*
         * VISUAL STRUCTURE (32 Nodes, 6 Levels):
         * Fashion (Root - Depth 0)
         * ├── Men (L1 - Depth 1)
         * │   ├── Men Clothing (L2 - Depth 2)
         * │   │   ├── Men Tops (L3 - Depth 3)
         * │   │   │   └── T-Shirts (L4 - Depth 4)
         * │   │   │       └── Oversized (L5 - Depth 5)
         * │   │   └── Men Bottoms (L3 - Depth 3)
         * │   │       └── Jeans (L4 - Depth 4)
         * │   │           └── Slim Fit (L5 - Depth 5)
         * │   └── Men Shoes (L2 - Depth 2)
         * │       ├── Casual Shoes (L3 - Depth 3)
         * │       │   └── Sneakers (L4 - Depth 4)
         * │       │       └── Running (L5 - Depth 5)
         * │       └── Formal Shoes (L3 - Depth 3)
         * │           └── Oxfords (L4 - Depth 4)
         * ├── Women (L1 - Depth 1)
         * │   ├── Women Clothing (L2 - Depth 2)
         * │   │   ├── Dresses (L3 - Depth 3)
         * │   │   │   └── Maxi Dresses (L4 - Depth 4)
         * │   │   └── Skirts (L3 - Depth 3)
         * │   │       └── Mini Skirts (L4 - Depth 4)
         * │   └── Women Accessories (L2 - Depth 2)
         * │       ├── Bags (L3 - Depth 3)
         * │       │   └── Handbags (L4 - Depth 4)
         * │       │       └── Clutches (L5 - Depth 5)
         * │       └── Jewelry (L3 - Depth 3)
         * │           └── Necklaces (L4 - Depth 4)
         * └── Kids (L1 - Depth 1)
         *     ├── Boys (L2 - Depth 2)
         *     │   └── Boys School (L3 - Depth 3)
         *     └── Girls (L2 - Depth 2)
         *         └── Girls Party (L3 - Depth 3)
         */

        // --- LAYER 0: ROOT ---
        var taxonomy = Taxonomy.Create("Fashion").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var rootId = (await fixture.Context.Set<Taxon>().FirstAsync(t => t.TaxonomyId == taxonomy.Id && t.ParentId == null, TestContext.Current.CancellationToken)).Id;

        // --- LAYER 1: (3 nodes) ---
        var men = Taxon.Create(taxonomy.Id, "Men", rootId).Value;
        var women = Taxon.Create(taxonomy.Id, "Women", rootId).Value;
        var kids = Taxon.Create(taxonomy.Id, "Kids", rootId).Value;
        fixture.Context.Set<Taxon>().AddRange(men, women, kids);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // --- LAYER 2: (6 nodes) ---
        var mCloth = Taxon.Create(taxonomy.Id, "Men Clothing", men.Id).Value;
        var mShoes = Taxon.Create(taxonomy.Id, "Men Shoes", men.Id).Value;
        var wCloth = Taxon.Create(taxonomy.Id, "Women Clothing", women.Id).Value;
        var wAcc = Taxon.Create(taxonomy.Id, "Women Accessories", women.Id).Value;
        var kBoys = Taxon.Create(taxonomy.Id, "Boys", kids.Id).Value;
        var kGirls = Taxon.Create(taxonomy.Id, "Girls", kids.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(mCloth, mShoes, wCloth, wAcc, kBoys, kGirls);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // --- LAYER 3: (10 nodes) ---
        var mTops = Taxon.Create(taxonomy.Id, "Men Tops", mCloth.Id).Value;
        var mBots = Taxon.Create(taxonomy.Id, "Men Bottoms", mCloth.Id).Value;
        var mCas = Taxon.Create(taxonomy.Id, "Casual Shoes", mShoes.Id).Value;
        var mForm = Taxon.Create(taxonomy.Id, "Formal Shoes", mShoes.Id).Value;
        var wDress = Taxon.Create(taxonomy.Id, "Dresses", wCloth.Id).Value;
        var wSkirt = Taxon.Create(taxonomy.Id, "Skirts", wCloth.Id).Value;
        var wBags = Taxon.Create(taxonomy.Id, "Bags", wAcc.Id).Value;
        var wJewel = Taxon.Create(taxonomy.Id, "Jewelry", wAcc.Id).Value;
        var kBSchool = Taxon.Create(taxonomy.Id, "Boys School", kBoys.Id).Value;
        var kGParty = Taxon.Create(taxonomy.Id, "Girls Party", kGirls.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(mTops, mBots, mCas, mForm, wDress, wSkirt, wBags, wJewel, kBSchool, kGParty);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // --- LAYER 4: (8 nodes) ---
        var tshirts = Taxon.Create(taxonomy.Id, "T-Shirts", mTops.Id).Value;
        var jeans = Taxon.Create(taxonomy.Id, "Jeans", mBots.Id).Value;
        var sneakers = Taxon.Create(taxonomy.Id, "Sneakers", mCas.Id).Value;
        var oxfords = Taxon.Create(taxonomy.Id, "Oxfords", mForm.Id).Value;
        var maxi = Taxon.Create(taxonomy.Id, "Maxi Dresses", wDress.Id).Value;
        var mini = Taxon.Create(taxonomy.Id, "Mini Skirts", wSkirt.Id).Value;
        var handBags = Taxon.Create(taxonomy.Id, "Handbags", wBags.Id).Value;
        var neck = Taxon.Create(taxonomy.Id, "Necklaces", wJewel.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(tshirts, jeans, sneakers, oxfords, maxi, mini, handBags, neck);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // --- LAYER 5: (4 nodes) ---
        var oversized = Taxon.Create(taxonomy.Id, "Oversized", tshirts.Id).Value;
        var slimFit = Taxon.Create(taxonomy.Id, "Slim Fit", jeans.Id).Value;
        var running = Taxon.Create(taxonomy.Id, "Running", sneakers.Id).Value;
        var clutches = Taxon.Create(taxonomy.Id, "Clutches", handBags.Id).Value;
        fixture.Context.Set<Taxon>().AddRange(oversized, slimFit, running, clutches);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // ACT
        var result = await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // ASSERT
        result.IsError.Should().BeFalse();
        var taxons = await fixture.Context.Set<Taxon>()
            .Where(t => t.TaxonomyId == taxonomy.Id)
            .ToDictionaryAsync(t => t.Name, TestContext.Current.CancellationToken);

        // Node check: 1 + 3 + 6 + 10 + 8 + 4 = 32
        taxons.Should().HaveCount(32);

        // Ultra Deep Path Check (Level 5)
        taxons["Running"].Permalink.Should().Be("fashion/men/men-shoes/casual-shoes/sneakers/running");
        taxons["Running"].PrettyName.Should().Be("Fashion -> Men -> Men Shoes -> Casual Shoes -> Sneakers -> Running");
        taxons["Running"].Depth.Should().Be(5);

        taxons["Oversized"].Permalink.Should().Be("fashion/men/men-clothing/men-tops/t-shirts/oversized");
        taxons["Clutches"].Permalink.Should().Be("fashion/women/women-accessories/bags/handbags/clutches");

        // Root range check
        taxons["Fashion"].Lft.Should().Be(1);
        taxons["Fashion"].Rgt.Should().Be(64); // 32 nodes * 2

        // Tree Mapping Check
        var tree = await _service.BuildTaxonTreeAsync(new TaxonQueryOptions { TaxonomyId = [taxonomy.Id] }, TestContext.Current.CancellationToken);
        tree.Tree.First().Children.Should().HaveCount(3); // Men, Women, Kids
    }

    [Fact(DisplayName = "Extreme Hierarchy: Should correctly calculate structure (>20 nodes, 5 depths, 1-5 children)")]
    public async Task Rebuild_ExtremeHierarchy_Success()
    {
        var taxonomy = Taxonomy.Create("ExtremeCatalog").Value;
        var root = taxonomy.RootTaxon!;
        int totalNodes = 1;

        for (int i = 1; i <= 4; i++)
        {
            var l1 = taxonomy.AddTaxon($"L1-{i}", root.Id).Value;
            totalNodes++;
            for (int j = 1; j <= 2; j++)
            {
                var l2 = taxonomy.AddTaxon($"L1-{i}-L2-{j}", l1.Id).Value;
                totalNodes++;
                int l3Count = (i + j) % 2 == 0 ? 2 : 1;
                for (int k = 1; k <= l3Count; k++)
                {
                    var l3 = taxonomy.AddTaxon($"L1-{i}-L2-{j}-L3-{k}", l2.Id).Value;
                    totalNodes++;
                    if (totalNodes < 25 && (i + j + k) % 2 == 0)
                    {
                        taxonomy.AddTaxon($"L1-{i}-L2-{j}-L3-{k}-L4", l3.Id);
                        totalNodes++;
                    }
                }
            }
        }

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);
        result.IsError.Should().BeFalse();

        var allTaxons = await fixture.Context.Set<Taxon>()
            .Where(t => t.TaxonomyId == taxonomy.Id)
            .ToDictionaryAsync(t => t.Name, TestContext.Current.CancellationToken);

        allTaxons.Count.Should().Be(totalNodes);
    }
}