using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.GetTaxonRules;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class GetTaxonRulesTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "Handle: Should return list of rules for taxon")]
    public async Task Handle_ValidRequest_ShouldReturnRules()
    {
        // Arrange
        var taxonomy = Taxonomy.Create($"RulesGet_{Guid.NewGuid()}").Value;
        var root = taxonomy.RootTaxon!;
        root.AddRule("product_name", "Test", "contains");
        
        fixture.Context.Set<Taxonomy>().Add(taxonomy);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetTaxonRules.Handler(fixture.Context);
        var query = new GetTaxonRules.Query(taxonomy.Id, root.Id);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Type.Should().Be("product_name");
    }
}