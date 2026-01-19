using ReSys.Shared.Models;
using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Shared.Models.Filters;
using ReSys.Shared.Models.Search;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

public class GranularOptionsTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact(DisplayName = "Granular: ApplyFilter should work standalone")]
    public void ApplyFilter_Standalone_Works()
    {
        var data = new List<TestItem>
        {
            new() { Name = "A", Value = 10 },
            new() { Name = "B", Value = 20 }
        }.AsQueryable();

        var options = new FilterOptions { Filter = "Value>15" };
        var result = data.ApplyFilter(options).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("B");
    }

    [Fact(DisplayName = "Granular: ApplyFilter with null options should return original query")]
    public void ApplyFilter_NullOptions_ReturnsOriginal()
    {
        var data = new List<TestItem> { new() { Name = "A" } }.AsQueryable();
        var result = data.ApplyFilter((IFilterOptions)null!);
        result.Should().BeSameAs(data);
    }

    [Fact(DisplayName = "Granular: ApplySort should work standalone")]
    public void ApplySort_Standalone_Works()
    {
        var data = new List<TestItem>
        {
            new() { Name = "A", Value = 20 },
            new() { Name = "B", Value = 10 }
        }.AsQueryable();

        var options = new SortOptions { Sort = "Value" };
        var result = data.ApplySort(options).ToList();

        result[0].Name.Should().Be("B"); // 10
    }

    [Fact(DisplayName = "Granular: ApplySort with empty sort string should return original")]
    public void ApplySort_EmptySort_ReturnsOriginal()
    {
        var data = new List<TestItem> { new() { Name = "A" } }.AsQueryable();
        var options = new SortOptions { Sort = "" };
        var result = data.ApplySort(options);
        result.Should().BeSameAs(data);
    }

    [Fact(DisplayName = "Granular: ApplySearch should work standalone")]
    public void ApplySearch_Standalone_Works()
    {
        var data = new List<TestItem>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" }
        }.AsQueryable();

        var options = new SearchOptions { Search = "pp", SearchField = new[] { "Name" } };
        var result = data.ApplySearch(options).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Granular: ApplySearch with multiple fields should match any")]
    public void ApplySearch_MultipleFields_MatchesAny()
    {
        var data = new List<TestItem>
        {
            new() { Name = "Apple", Value = 100 },
            new() { Name = "Banana", Value = 200 }
        }.AsQueryable();

        // Search for "200" in Name or Value (Value.ToString())
        var options = new SearchOptions { Search = "200", SearchField = new[] { "Name", "Value" } };
        var result = data.ApplySearch(options).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Banana");
    }

    [Fact(DisplayName = "Granular: ApplyPagingAsync should work standalone")]
    public async Task ApplyPagingAsync_Standalone_Works()
    {
        // Using EF Core for async paging
        var baseName = $"GranularPage_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_1" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_2" },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_3" }
        );
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = fixture.Context.Set<Example>().Where(x => x.Name.StartsWith(baseName));
        var options = new PageOptions { Page = 1, PageSize = 2 };
        
        // Specify projection and types explicitly if inference fails
        var result = await query.ApplyPagingAsync(options, x => x, TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "Granular: ApplyPagingAsync with custom projection")]
    public async Task ApplyPagingAsync_WithProjection_Works()
    {
        var baseName = $"GranularProj_{Guid.NewGuid()}";
        fixture.Context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = $"{baseName}_X" });
        await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = fixture.Context.Set<Example>().Where(x => x.Name == $"{baseName}_X");
        var options = new PageOptions { Page = 1, PageSize = 10 };

        var result = await query.ApplyPagingAsync(options, x => new { x.Name }, TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be($"{baseName}_X");
    }

    [Fact(DisplayName = "Granular: QueryBuilder should build specific options")]
    public void QueryBuilder_BuildsGranularOptions()
    {
        var builder = new QueryBuilder<TestItem>()
            .Where("Value", ">", 10)
            .OrderBy("Name")
            .Search("test", "Name")
            .Page(2, 5);

        var filterOpts = builder.BuildFilterOptions();
        var sortOpts = builder.BuildSortOptions();
        var searchOpts = builder.BuildSearchOptions();
        var pageOpts = builder.BuildPageOptions();

        filterOpts.Filter.Should().Be("Value>10");
        sortOpts.Sort.Should().Be("Name");
        searchOpts.Search.Should().Be("test");
        pageOpts.Page.Should().Be(2);
    }
}
