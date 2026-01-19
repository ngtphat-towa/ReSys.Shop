using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.UpdateTaxonomy;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class UpdateTaxonomyTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly ITaxonHierarchyService _hierarchyService = Substitute.For<ITaxonHierarchyService>();

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

    [Fact(DisplayName = "Handle: Should return error when name is duplicate")]
    public async Task Handle_DuplicateName_ShouldReturnError()
    {
        // Arrange
        var name1 = $"Tax1_{Guid.NewGuid()}";
        var name2 = $"Tax2_{Guid.NewGuid()}";
        var t1 = Taxonomy.Create(name1).Value;
        var t2 = Taxonomy.Create(name2).Value;
        fixture.Context.Set<Taxonomy>().AddRange(t1, t2);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateTaxonomy.Handler(fixture.Context);
        var request = new UpdateTaxonomy.Request { Name = name2, Presentation = "P" };
        var command = new UpdateTaxonomy.Command(t1.Id, request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TaxonomyErrors.DuplicateName);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when taxonomy does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new UpdateTaxonomy.Handler(fixture.Context);
        var request = new UpdateTaxonomy.Request { Name = "Valid", Presentation = "P" };
        var command = new UpdateTaxonomy.Command(Guid.NewGuid(), request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }
}
