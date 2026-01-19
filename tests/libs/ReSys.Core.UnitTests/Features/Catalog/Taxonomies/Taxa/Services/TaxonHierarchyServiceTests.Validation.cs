using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

using Microsoft.Extensions.Logging.Abstractions;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyValidationTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "ValidateHierarchy: Should succeed for a valid single-root hierarchy")]
    public async Task Validate_HappyPath_Success()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("ValidTaxonomy").Value;
        taxonomy.AddTaxon("Child 1");
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact(DisplayName = "ValidateHierarchy: Should fail if multiple roots are detected")]
    public async Task Validate_MultipleRoots_Failure()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("MultiRoot").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Bypass domain logic to add a second root
        var secondRoot = Taxon.Create(taxonomy.Id, "SecondRoot").Value;
        fixture.Context.Set<Taxon>().Add(secondRoot);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxon.RootConflict");
    }

    [Fact(DisplayName = "ValidateHierarchy: Should fail if a cycle is detected")]
    public async Task Validate_Cycle_Failure()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("CycleTaxonomy").Value;
        var root = taxonomy.RootTaxon!;
        var child = taxonomy.AddTaxon("Child").Value;

        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Bypass domain logic to create a cycle: root -> child -> root
        root.ParentId = child.Id;
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxon.CycleDetected");
    }

    [Fact(DisplayName = "ValidateHierarchy: Should fail if no root exists")]
    public async Task Validate_NoRoot_Failure()
    {
        // Arrange: Use a new ID that doesn't exist in the DB to simulate "No Root Found"
        var fakeTaxonomyId = Guid.NewGuid();

        // Act
        var result = await _service.ValidateHierarchyAsync(fakeTaxonomyId, TestContext.Current.CancellationToken);

        // Assert
        // In our service, 0 taxons found returns success. 
        // To trigger NoRoot, we need a taxonomy that HAS taxons but NONE of them are roots.

        // Let's create a taxonomy with a taxon that incorrectly has a ParentId (orphan)
        var taxonomy = Taxonomy.Create("OrphanTaxonomy").Value;
        var orphan = Taxon.Create(taxonomy.Id, "Orphan").Value;
        orphan.ParentId = Guid.NewGuid(); // Orphaned

        fixture.Context.Set<Taxon>().Add(orphan);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var resultOrphan = await _service.ValidateHierarchyAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        resultOrphan.IsError.Should().BeTrue();
        resultOrphan.FirstError.Code.Should().Be("Taxon.NoRoot");
    }
}