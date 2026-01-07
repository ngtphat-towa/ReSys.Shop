using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Models;
using ReSys.Core.Domain;
using ReSys.Core.UnitTests.TestInfrastructure;

using Xunit;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

public class QueryExtensionsChainTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public QueryExtensionsChainTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private class TestItem
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    [Fact(DisplayName = "ApplyQueryOptions: Should return IQueryable for chaining")]
    public void ApplyQueryOptions_ReturnsIQueryable()
    {
        var data = new List<TestItem>
        {
            new() { Name = "A" },
            new() { Name = "B" }
        }.AsQueryable();

        var options = new QueryOptions { Filter = "Name=A" };
        var query = data.ApplyQueryOptions(options);

        // Chain more LINQ
        var result = query.Where(x => x.Name.Length == 1).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("A");
    }

    [Fact(DisplayName = "ApplyQueryOptions: Should handle mixed criteria on in-memory data")]
    public void ApplyQueryOptions_MixedCriteria_Works()
    {
        var data = new List<TestItem>
        {
            new() { Name = "Apple", Value = 10 },
            new() { Name = "Banana", Value = 20 },
            new() { Name = "Cherry", Value = 30 }
        }.AsQueryable();

        var options = new QueryOptions
        {
            Filter = "Value>15",
            Sort = "Name desc",
            Search = "ry",
            SearchField = new[] { "Name" }
        };

        var result = data.ApplyQueryOptions(options).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Cherry");
    }

    [Fact(DisplayName = "ApplyQueryOptions: Should not throw on null options")]
    public void ApplyQueryOptions_NullOptions_ReturnsOriginal()
    {
        var data = new List<TestItem> { new() { Name = "A" } }.AsQueryable();
        var result = data.ApplyQueryOptions(null!);
        result.Should().BeSameAs(data);
    }

    [Fact(DisplayName = "ApplyQueryOptionsAsync: Should apply all criteria correctly (EF Core)")]
    public async Task ApplyQueryOptionsAsync_FullChain_Works()
    {
        var baseName = $"FullChain_{Guid.NewGuid()}";
        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_C", Price = 30 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_A", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_B", Price = 20 }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = _fixture.Context.Set<Example>().Where(x => x.Name.StartsWith(baseName));

        var options = new QueryOptions
        {
            Filter = "Price>15",
            Sort = "Name desc",
            Page = 1,
            PageSize = 1
        };

        var result = await query.ApplyQueryOptionsAsync(options, TestContext.Current.CancellationToken);

        // Filter: B, C. Sort Desc: C, B. Page 1, Size 1: C.
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be($"{baseName}_C");
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Filter: Should handle DateTimeOffset with Z suffix (UTC)")]
    public void Filter_DateTimeOffset_WithZ_ReturnsCorrectItems()
    {
        var utcDate = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var data = new List<TestItem>
        {
            new() { Name = "Match", CreatedAt = utcDate },
            new() { Name = "NoMatch", CreatedAt = utcDate.AddHours(1) }
        }.AsQueryable();

        // 2023-01-01T12:00:00Z
        var filter = "CreatedAt=2023-01-01T12:00:00Z";
        var result = data.ApplyDynamicFilter(filter).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Match");
    }
}
