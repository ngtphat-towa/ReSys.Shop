using NSubstitute;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace ReSys.Core.UnitTests.Features.Catalog.Taxonomies.Taxa.Rules;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class UpdateTaxonRulesTests : IDisposable
{
    private readonly ITaxonRegenerationService _regenerationService;
    private readonly TestAppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _options;

    public UpdateTaxonRulesTests()
    {
        _regenerationService = Substitute.For<ITaxonRegenerationService>();
        
        // Create a unique in-memory database for each test instance
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

    private async Task<Guid> AddRuleAsync(Guid taxonId, string type, string value, string matchPolicy, string? propertyName = null)
    {
        // Load the taxon with its rules
        var taxon = await _context.Set<Taxon>()
            .Include(t => t.TaxonRules)
            .FirstAsync(t => t.Id == taxonId);
        
        // Add the rule (this modifies the taxon entity)
        var ruleResult = taxon.AddRule(type, value, matchPolicy, propertyName);
        if (ruleResult.IsError)
        {
            throw new InvalidOperationException($"Failed to add rule: {ruleResult.FirstError.Description}");
        }
        
        // Explicitly track the new rule for InMemory provider
        _context.Add(ruleResult.Value);

        // Save changes while the entity is still tracked
        await _context.SaveChangesAsync();
        
        // Return the rule ID
        return ruleResult.Value.Id;
    }

    [Fact(DisplayName = "Handle: Should add new rule when list is empty")]
    public async Task Handle_AddRule_ShouldSucceed()
    {
        // Arrange
        var (taxonomyId, rootId) = await SetupTaxonomyAsync($"AddRule_{Guid.NewGuid()}");
        
        _regenerationService.RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        var handler = new UpdateTaxonRules.Handler(_context, _regenerationService, NullLogger<UpdateTaxonRules.Handler>.Instance);

        var request = new UpdateTaxonRules.Request
        {
            Rules = [new TaxonRuleInput { Type = "product_name", Value = "NewRule", MatchPolicy = "contains" }]
        };

        // Act
        var result = await handler.Handle(
            new UpdateTaxonRules.Command(taxonomyId, rootId, request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Rules.Should().HaveCount(1);
        result.Value.Rules[0].Value.Should().Be("NewRule");

        await _regenerationService.Received(1)
            .RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should update existing rule when ID matches")]
    public async Task Handle_UpdateRule_ShouldSucceed()
    {
        // Arrange
        var (taxonomyId, rootId) = await SetupTaxonomyAsync($"UpdateRule_{Guid.NewGuid()}");
        var ruleId = await AddRuleAsync(rootId, "product_name", "OldValue", "contains");

        _regenerationService.RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        var handler = new UpdateTaxonRules.Handler(_context, _regenerationService, NullLogger<UpdateTaxonRules.Handler>.Instance);

        var request = new UpdateTaxonRules.Request
        {
            Rules = [new TaxonRuleInput { Id = ruleId, Type = "product_name", Value = "UpdatedValue", MatchPolicy = "contains" }]
        };

        // Act
        var result = await handler.Handle(
            new UpdateTaxonRules.Command(taxonomyId, rootId, request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Rules.Should().HaveCount(1);
        result.Value.Rules[0].Value.Should().Be("UpdatedValue");

        await _regenerationService.Received(1)
            .RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should remove rule when not in request list")]
    public async Task Handle_RemoveRule_ShouldSucceed()
    {
        // Arrange
        var (taxonomyId, rootId) = await SetupTaxonomyAsync($"RemoveRule_{Guid.NewGuid()}");
        await AddRuleAsync(rootId, "product_name", "ToBeRemoved", "contains");

        _regenerationService.RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        var handler = new UpdateTaxonRules.Handler(_context, _regenerationService, NullLogger<UpdateTaxonRules.Handler>.Instance);

        var request = new UpdateTaxonRules.Request
        {
            Rules = [] // Empty list
        };

        // Act
        var result = await handler.Handle(
            new UpdateTaxonRules.Command(taxonomyId, rootId, request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Rules.Should().BeEmpty();

        await _regenerationService.Received(1)
            .RegenerateProductsForTaxonAsync(rootId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should not regenerate if no changes made")]
    public async Task Handle_NoChanges_ShouldNotRegenerate()
    {
        // Arrange
        var (taxonomyId, rootId) = await SetupTaxonomyAsync($"NoChange_{Guid.NewGuid()}");
        var ruleId = await AddRuleAsync(rootId, "product_name", "Stable", "contains");

        var handler = new UpdateTaxonRules.Handler(_context, _regenerationService, NullLogger<UpdateTaxonRules.Handler>.Instance);

        var request = new UpdateTaxonRules.Request
        {
            Rules = [new TaxonRuleInput { Id = ruleId, Type = "product_name", Value = "Stable", MatchPolicy = "contains" }]
        };

        // Act
        var result = await handler.Handle(
            new UpdateTaxonRules.Command(taxonomyId, rootId, request), 
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _regenerationService.DidNotReceive()
            .RegenerateProductsForTaxonAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}