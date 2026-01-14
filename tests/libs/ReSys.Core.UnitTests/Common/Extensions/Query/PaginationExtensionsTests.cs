using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Query")]
public class PaginationExtensionsTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact(DisplayName = "ToPagedListAsync: Should return first page with default size when both parameters are null")]
    public async Task ToPagedListAsync_Should_ReturnFirstPageWithDefaultSize_WhenParametersAreNull()
    {
        var baseName = $"PagedNull_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id, p.Name }, page: null, pageSize: null, TestContext.Current.CancellationToken);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should parse dictionary-based parameters correctly")]
    public async Task ToPagedListAsync_Should_ParseParameters_WhenDictionaryProvided()
    {
        var baseName = $"Dict_{Guid.NewGuid()}";
        await SeedExamples(baseName, 20);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var parameters = new Dictionary<string, string?>
        {
            { "page_index", "2" },
            { "page_size", "5" }
        };

        var result = await query.ToPagedListAsync(p => new { p.Id, p.Name }, parameters, TestContext.Current.CancellationToken);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should fallback to default page when index is negative")]
    public async Task ToPagedListAsync_Should_FallbackToDefault_WhenPageIndexIsNegative()
    {
        var baseName = $"NegPage_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, -1, 10, TestContext.Current.CancellationToken);
        
        result.Page.Should().Be(1);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should fallback to default size when page size is negative")]
    public async Task ToPagedListAsync_Should_FallbackToDefault_WhenPageSizeIsNegative()
    {
        var baseName = $"NegSize_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, 1, -10, TestContext.Current.CancellationToken);
        
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    [Fact(DisplayName = "ToPagedListAsync: Should return empty items when page index exceeds total results")]
    public async Task ToPagedListAsync_Should_ReturnEmpty_WhenPageIndexExceedsTotal()
    {
        var baseName = $"Exceed_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, 100, 10, TestContext.Current.CancellationToken);
        
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return all items when both parameters are null")]
    public async Task ToPagedOrAllAsync_Should_ReturnAllItems_WhenParametersAreNull()
    {
        var baseName = $"AllItems_{Guid.NewGuid()}";
        await SeedExamples(baseName, 25);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedOrAllAsync(p => new { p.Id, p.Name }, page: null, pageSize: null, cancellationToken: TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(25);
        result.TotalCount.Should().Be(25);
    }

    [Fact(DisplayName = "ToPagedOrAllAsync: Should return paged result when parameters are provided")]
    public async Task ToPagedOrAllAsync_Should_ReturnPagedResult_WhenParametersProvided()
    {
        var baseName = $"PagedOrAll_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedOrAllAsync(p => new { p.Id }, 1, 5, TestContext.Current.CancellationToken);
        
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
    }

    private async Task SeedExamples(string baseName, int count)
    {
        var Examples = Enumerable.Range(1, count)
            .Select(i => new Example
            {
                Id = Guid.NewGuid(),
                Name = $"{baseName}_{i:D3}",
                Price = i * 10
            })
            .ToList();

        fixture.Context.Set<Example>().AddRange(Examples);
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
