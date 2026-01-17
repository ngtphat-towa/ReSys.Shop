using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

using Microsoft.Extensions.Logging.Abstractions;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class TaxonHierarchyRebuildTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly TaxonHierarchyService _service = new(fixture.Context, NullLogger<TaxonHierarchyService>.Instance);

    [Fact(DisplayName = "Rebuild: Should orchestrate all rebuild steps successfully")]
    public async Task Rebuild_Full_ShouldSucceed()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("FullRebuild").Value;
        taxonomy.AddTaxon("Level 1");
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.RebuildAsync(taxonomy.Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact(DisplayName = "Rebuild: Should return error if orchestration fails at validation")]
    public async Task Rebuild_Invalid_ShouldReturnError()
    {
        // Arrange
        var taxonomy = Taxonomy.Create("FailRebuild").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Guid.Empty is invalid
        var result = await _service.RebuildAsync(Guid.Empty, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
    }
}