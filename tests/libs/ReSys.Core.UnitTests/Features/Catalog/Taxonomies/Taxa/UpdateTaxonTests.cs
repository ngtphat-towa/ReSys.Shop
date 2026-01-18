using NSubstitute;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxon;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class UpdateTaxonTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly ITaxonHierarchyService _hierarchyService = Substitute.For<ITaxonHierarchyService>();
    private readonly ITaxonRegenerationService _regenerationService = Substitute.For<ITaxonRegenerationService>();

    [Fact(DisplayName = "Handle: Should prevent moving root taxon")]
    public async Task Handle_MoveRoot_ShouldFail()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"RootLock_{Guid.NewGuid()}").Value;
        var root = taxonomy.RootTaxon!;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateTaxon.Handler(fixture.Context);
        var request = new UpdateTaxon.Request
        {
            Name = "NewRootName",
            Presentation = "NewRootName",
            ParentId = Guid.NewGuid() // Attempt to re-parent root
        };
        var command = new UpdateTaxon.Command(taxonomy.Id, root.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxon.RootLock");
    }

    [Fact(DisplayName = "Handle: Should return NotFound when taxon does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"Taxon404_{Guid.NewGuid()}").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateTaxon.Handler(fixture.Context);
        var request = new UpdateTaxon.Request { Name = "Valid", Presentation = "Valid" };
        var command = new UpdateTaxon.Command(taxonomy.Id, Guid.NewGuid(), request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
