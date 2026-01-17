using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.UpdateTaxonomy;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.UpdateTaxonomyTests;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class UpdateTaxonomyTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should update state and sync root taxon")]
    public async Task Handle_ValidRequest_ShouldSyncRoot()
    {
        // Arrange
        var baseName = $"OldName_{Guid.NewGuid()}";
        var taxonomy = Taxonomy.Create(baseName).Value;
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateTaxonomy.Handler(fixture.Context);
        var request = new UpdateTaxonomy.Request 
        { 
            Name = $"NewName_{Guid.NewGuid()}", 
            Presentation = "New Presentation",
            Position = 5
        };
        var command = new UpdateTaxonomy.Command(taxonomy.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        
        var rootTaxon = await fixture.Context.Set<Taxon>()
            .FirstOrDefaultAsync(t => t.TaxonomyId == taxonomy.Id && t.ParentId == null, TestContext.Current.CancellationToken);
        
        rootTaxon!.Name.Should().Be(request.Name);
        rootTaxon.Permalink.Should().Be(request.Name.ToLower().Replace(" ", "-"));
    }
}