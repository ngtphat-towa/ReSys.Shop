using NSubstitute;

using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.CreateTaxon;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class CreateTaxonTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{

    [Fact(DisplayName = "Handle: Should create taxon successfully")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"TaxonTest_{Guid.NewGuid()}").Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var rootId = taxonomy.RootTaxon!.Id;

        var handler = new CreateTaxon.Handler(fixture.Context);
        var request = new CreateTaxon.Request
        {
            Name = "ChildNode",
            Presentation = "Child Node",
            ParentId = rootId
        };
        var command = new CreateTaxon.Command(taxonomy.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("ChildNode");
    }
}
