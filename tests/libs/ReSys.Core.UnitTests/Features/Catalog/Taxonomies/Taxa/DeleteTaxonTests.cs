using NSubstitute;

using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.DeleteTaxon;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class DeleteTaxonTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly ITaxonHierarchyService _hierarchyService = Substitute.For<ITaxonHierarchyService>();

    [Fact(DisplayName = "Handle: Should delete taxon and rebuild hierarchy")]
    public async Task Handle_ValidId_ShouldSucceed()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"DeleteTaxon_{Guid.NewGuid()}").Value;
        var root = taxonomy.RootTaxon!;
        var child = taxonomy.AddTaxon("ToDelete").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteTaxon.Handler(fixture.Context, _hierarchyService);
        var command = new DeleteTaxon.Command(taxonomy.Id, child.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _hierarchyService.Received(1).RebuildAsync(taxonomy.Id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should prevent deleting root taxon")]
    public async Task Handle_DeleteRoot_ShouldFail()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"RootDelete_{Guid.NewGuid()}").Value;
        var root = taxonomy.RootTaxon!;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteTaxon.Handler(fixture.Context, _hierarchyService);
        var command = new DeleteTaxon.Command(taxonomy.Id, root.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxon.RootLock");
    }
}