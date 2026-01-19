using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.DeleteTaxonomy;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class DeleteTaxonomyTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should fail if child taxons exist beyond root")]
    public async Task Handle_WithChildren_ShouldFail()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"Locked_{Guid.NewGuid()}").Value;
        taxonomy.AddTaxon("Level 1 Child");
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteTaxonomy.Handler(fixture.Context);
        var command = new DeleteTaxonomy.Command(taxonomy.Id);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Taxonomy.HasTaxons");
    }

}
