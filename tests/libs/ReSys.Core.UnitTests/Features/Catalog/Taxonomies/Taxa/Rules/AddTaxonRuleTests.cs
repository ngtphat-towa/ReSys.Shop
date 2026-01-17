using NSubstitute;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.AddTaxonRule;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class AddTaxonRuleTests : IDisposable
{
    private readonly TestAppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _options;

    public AddTaxonRuleTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new TestAppDbContext(_options);
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }

    private async Task<(Guid TaxonomyId, Guid RootTaxonId)> SetupTaxonomyAsync(string name)
    {
        var taxonomy = Taxonomy.Create(name).Value;
        _context.Set<Taxonomy>().Add(taxonomy);
        await _context.SaveChangesAsync();
        
        return (taxonomy.Id, taxonomy.RootTaxon!.Id);
    }

    [Fact(DisplayName = "Handle: Should successfully add a new rule")]
    public async Task Handle_AddRule_ShouldSucceed()
    {
        // Arrange
        var (taxonomyId, rootId) = await SetupTaxonomyAsync($"AddRule_{Guid.NewGuid()}");
        
        var handler = new AddTaxonRule.Handler(_context);

        var request = new AddTaxonRule.Request
        {
            Type = "product_name",
            Value = "TestValue",
            MatchPolicy = "contains"
        };

        // Act
        var result = await handler.Handle(
            new AddTaxonRule.Command(taxonomyId, rootId, request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("TestValue");

        // Verify in DB
        var taxon = await _context.Set<Taxon>()
            .Include(t => t.TaxonRules)
            .FirstAsync(t => t.Id == rootId, TestContext.Current.CancellationToken);
        
        taxon.TaxonRules.Should().HaveCount(1);
        taxon.TaxonRules.First().Value.Should().Be("TestValue");
    }

    [Fact(DisplayName = "Handle: Should return NotFound when taxon does not exist")]
    public async Task Handle_TaxonNotFound_ShouldReturnError()
    {
        // Arrange
        var handler = new AddTaxonRule.Handler(_context);
        var request = new AddTaxonRule.Request { Type = "product_name", Value = "Val" };

        // Act
        var result = await handler.Handle(
            new AddTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
