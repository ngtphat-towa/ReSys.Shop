using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class CreateTaxonomyTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly ITaxonHierarchyService _hierarchyService = Substitute.For<ITaxonHierarchyService>();

    [Fact(DisplayName = "Handle: Should create taxonomy and automatic root taxon")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var handler = new CreateTaxonomy.Handler(fixture.Context);
        var request = new CreateTaxonomy.Request
        {
            Name = $"NewTaxonomy_{Guid.NewGuid()}",
            Presentation = "New Presentation"
        };
        var command = new CreateTaxonomy.Command(request);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(request.Name);

        // Verify Root Taxon exists in DB
        var rootTaxon = await fixture.Context.Set<Taxon>()
            .FirstOrDefaultAsync(t => t.TaxonomyId == result.Value.Id && t.ParentId == null, TestContext.Current.CancellationToken);

        rootTaxon.Should().NotBeNull();
        rootTaxon!.Name.Should().Be(request.Name);
    }
}
