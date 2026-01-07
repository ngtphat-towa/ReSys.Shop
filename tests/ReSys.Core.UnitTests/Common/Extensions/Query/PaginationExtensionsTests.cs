using FluentAssertions;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Domain;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

public class PaginationExtensionsTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public PaginationExtensionsTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Pagination: Should return first page with default size when both params are null")]
    public async Task ToPagedListAsync_BothNull_ShouldReturnFirstPageWithDefaultSize()
    {
        var baseName = $"PagedNull_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id, p.Name }, page: null, pageSize: null, TestContext.Current.CancellationToken);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact(DisplayName = "Pagination: Dictionary-based params should be parsed correctly")]
    public async Task ToPagedListAsync_WithDictionary_ShouldParseParams()
    {
        var baseName = $"Dict_{Guid.NewGuid()}";
        await SeedExamples(baseName, 20);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

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

    [Fact(DisplayName = "Pagination: Negative page index should fallback to default")]
    public async Task ToPagedListAsync_NegativePage_ReturnsDefault()
    {
        var baseName = $"NegPage_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, -1, 10, TestContext.Current.CancellationToken);
        
        result.Page.Should().Be(1);
    }

    [Fact(DisplayName = "Pagination: Negative page size should fallback to default")]
    public async Task ToPagedListAsync_NegativeSize_ReturnsDefault()
    {
        var baseName = $"NegSize_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, 1, -10, TestContext.Current.CancellationToken);
        
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    [Fact(DisplayName = "Pagination: Page index exceeding total count should return empty")]
    public async Task ToPagedListAsync_ExceedingPage_ReturnsEmpty()
    {
        var baseName = $"Exceed_{Guid.NewGuid()}";
        await SeedExamples(baseName, 5);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedListAsync(p => new { p.Id }, 100, 10, TestContext.Current.CancellationToken);
        
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact(DisplayName = "Pagination (All): Should return everything when both params are null")]
    public async Task ToPagedOrAllAsync_BothNull_ShouldReturnAllItems()
    {
        var baseName = $"AllItems_{Guid.NewGuid()}";
        await SeedExamples(baseName, 25);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

        var result = await query.ToPagedOrAllAsync(p => new { p.Id, p.Name }, page: null, pageSize: null, cancellationToken: TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(25);
        result.TotalCount.Should().Be(25);
    }

    [Fact(DisplayName = "Pagination (All): Should return paged result when params are provided")]
    public async Task ToPagedOrAllAsync_WithParams_ReturnsPaged()
    {
        var baseName = $"PagedOrAll_{Guid.NewGuid()}";
        await SeedExamples(baseName, 15);
        var query = _fixture.Context.Set<Example>().Where(p => p.Name.StartsWith(baseName));

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

        _fixture.Context.Set<Example>().AddRange(Examples);
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
