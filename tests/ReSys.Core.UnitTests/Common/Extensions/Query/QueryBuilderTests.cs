using FluentAssertions;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Domain;
using ReSys.Core.UnitTests.TestInfrastructure;

using Xunit;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

public class QueryBuilderTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public QueryBuilderTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
    }

    [Fact(DisplayName = "QueryBuilder: Should build correct QueryOptions")]
    public void QueryBuilder_BuildsCorrectOptions()
    {
        var builder = new QueryBuilder<TestItem>();
        builder
            .Where(x => x.Name, "=", "Apple")
            .OrderByDescending(x => x.Price)
            .Page(2, 20)
            .Search("fruit", "Name");

        var options = builder.Build();

        options.Filter.Should().Be("Name=Apple");
        options.Sort.Should().Be("Price desc");
        options.Page.Should().Be(2);
        options.PageSize.Should().Be(20);
        options.Search.Should().Be("fruit");
        options.SearchField.Should().Contain("Name");
    }

    [Fact(DisplayName = "QueryBuilder: Chaining methods should work correctly")]
    public void QueryBuilder_Chaining_Works()
    {
        var builder = new QueryBuilder<TestItem>()
            .Where("Price", ">", 10)
            .Where("Name", "*", "a")
            .OrderBy("Name");

        var options = builder.Build();
        options.Filter.Should().Be("Price>10,Name*a");
        options.Sort.Should().Be("Name");
    }

    [Fact(DisplayName = "QueryBuilder: Adding raw filter should append to existing conditions")]
    public void QueryBuilder_AddRawFilter_Appends()
    {
        var builder = new QueryBuilder<TestItem>()
            .Where("Price", ">", 10)
            .AddRawFilter("IsActive=true");

        var options = builder.Build();
        options.Filter.Should().Be("Price>10,IsActive=true");
    }

    [Fact(DisplayName = "QueryBuilder: Search with member expressions should resolve property names")]
    public void QueryBuilder_SearchWithExpressions_Works()
    {
        var builder = new QueryBuilder<TestItem>()
            .Search("apple", x => x.Name);

        var options = builder.Build();
        options.SearchField.Should().Contain("Name");
    }

    [Fact(DisplayName = "QueryBuilder: Custom mapping should replace property names")]
    public void QueryBuilder_AddMap_Works()
    {
        var builder = new QueryBuilder<TestItem>()
            .AddMap("p", "Price")
            .AddMap("n", x => x.Name)
            .Where("p", ">", 10)
            .OrderBy("n");

        var options = builder.Build();
        options.Filter.Should().Be("Price>10");
        options.Sort.Should().Be("Name");
    }

    [Fact(DisplayName = "QueryBuilder: Grouping should generate correct parentheses")]
    public void QueryBuilder_Grouping_Works()
    {
        // (Price > 10 OR Name = Apple) AND Price < 50
        var builder = new QueryBuilder<TestItem>()
            .StartGroup()
            .Where("Price", ">", 10)
            .Or()
            .Where("Name", "=", "Apple")
            .EndGroup()
            .Where("Price", "<", 50);

        var options = builder.Build();
        options.Filter.Should().Be("(Price>10|Name=Apple),Price<50");
    }

    [Fact(DisplayName = "QueryBuilder: IsValid should detect invalid sort properties")]
    public void QueryBuilder_IsValid_DetectsInvalidSort()
    {
        var builder = new QueryBuilder<TestItem>()
            .OrderBy("InvalidProp");

        bool valid = builder.IsValid(out var errors);
        valid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("InvalidProp"));
    }

    [Fact(DisplayName = "QueryBuilder: Multiple sort fields should be comma separated")]
    public void QueryBuilder_MultipleSorts_Works()
    {
        var builder = new QueryBuilder<TestItem>()
            .OrderBy("Name")
            .OrderByDescending("Price");

        var options = builder.Build();
        options.Sort.Should().Be("Name,Price desc");
    }

    [Fact(DisplayName = "QueryBuilder: Complex grouping and OR logic")]
    public void QueryBuilder_ComplexLogic_Works()
    {
        // (Name=A | Name=B) | (Price>10, Price<20)
        var builder = new QueryBuilder<TestItem>()
            .StartGroup()
            .Where(x => x.Name, "=", "A")
            .Or()
            .Where(x => x.Name, "=", "B")
            .EndGroup()
            .Or()
            .StartGroup()
            .Where(x => x.Price, ">", 10)
            .Where(x => x.Price, "<", 20)
            .EndGroup();

        var options = builder.Build();
        options.Filter.Should().Be("(Name=A|Name=B)|(Price>10,Price<20)");
    }

    [Fact(DisplayName = "QueryBuilder: Should format different value types correctly")]
    public void QueryBuilder_FormatValues_Works()
    {
        var date = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var builder = new QueryBuilder<Example>()
            .Where("IsActive", "=", true)
            .Where("Price", "=", null)
            .Where("CreatedAt", ">", date);

        var options = builder.Build();
        // Booleans are lowercase, null is "null", dates are ISO 8601
        options.Filter.Should().Be("IsActive=true,Price=null,CreatedAt>2024-01-01T12:00:00.0000000Z");
    }

    [Fact(DisplayName = "QueryBuilder: Should handle deep nested properties in expressions")]
    public void QueryBuilder_NestedProperties_Works()
    {
        var builder = new QueryBuilder<Example>()
            .OrderBy(x => x.Category!.Name)
            .Where(x => x.Category!.Id, "=", Guid.Empty);

        var options = builder.Build();
        options.Sort.Should().Be("Category.Name");
        options.Filter.Should().Be($"Category.Id={Guid.Empty}");
    }

        [Fact(DisplayName = "QueryBuilder: IsValid should check all sort and search fields")]
        public void QueryBuilder_IsValid_ChecksEverything()
        {
            var builder = new QueryBuilder<TestItem>()
                .OrderBy("Name") 
                .OrderBy("InvalidProp1")
                .Search("test", "InvalidProp2");
    
            builder.IsValid(out var errors).Should().BeFalse();
            errors.Should().HaveCount(2);
            errors.Should().Contain(e => e.Contains("InvalidProp1"));
            errors.Should().Contain(e => e.Contains("InvalidProp2"));
        }
    
        [Fact(DisplayName = "QueryBuilder: IsValid should return true when everything is valid")]
        public void QueryBuilder_IsValid_Success()
        {
            var builder = new QueryBuilder<Example>()
                .OrderBy(x => x.Name)
                .Search("test", x => x.Description!, x => x.Category!.Name)
                .Where(x => x.Price, ">", 10);
    
            builder.IsValid(out var errors).Should().BeTrue();
            errors.Should().BeEmpty();
        }
    
        [Fact(DisplayName = "QueryBuilder: AddMap should support nested property paths")]
        public void QueryBuilder_AddMap_Nested_Works()
        {
            var builder = new QueryBuilder<Example>()
                .AddMap("cat", x => x.Category!.Name)
                .Where("cat", "=", "Books");
    
            var options = builder.Build();
            options.Filter.Should().Be("Category.Name=Books");
        }
    
        [Fact(DisplayName = "QueryBuilder: Should build granular options separately")]
        public void QueryBuilder_BuildGranular_Works()
        {
            var builder = new QueryBuilder<TestItem>()
                .Where(x => x.Name, "=", "A")
                .OrderBy(x => x.Price)
                .Search("term", x => x.Name)
                .Page(1, 10);
    
            var filter = builder.BuildFilterOptions();
            var sort = builder.BuildSortOptions();
            var search = builder.BuildSearchOptions();
            var page = builder.BuildPageOptions();
    
            filter.Filter.Should().Be("Name=A");
            sort.Sort.Should().Be("Price");
            search.Search.Should().Be("term");
            search.SearchField.Should().Contain("Name");
            page.Page.Should().Be(1);
            page.PageSize.Should().Be(10);
        }
    
        [Fact(DisplayName = "QueryBuilder: Should format DateTimeOffset and Enums correctly")]
        public void QueryBuilder_FormatSpecialTypes_Works()
        {
            var dto = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
            var builder = new QueryBuilder<Example>()
                .Where(x => x.Status, "=", ExampleStatus.Active)
                .Where(x => x.CreatedAt, ">", dto);
    
            var options = builder.Build();
            // Enums use ToString(), DateTimeOffset uses "o"
            options.Filter.Should().Be($"Status=Active,CreatedAt>{dto:o}");
        }
    
        [Fact(DisplayName = "QueryBuilder: Search with multiple expressions should collect all fields")]
        public void QueryBuilder_Search_MultipleExpressions_Works()
        {
            var builder = new QueryBuilder<Example>()
                .Search("query", x => x.Name, x => x.Description!, x => x.Category!.Name);
    
            var options = builder.Build();
            options.SearchField.Should().HaveCount(3);
            options.SearchField.Should().Contain(new[] { "Name", "Description", "Category.Name" });
        }
    
        [Fact(DisplayName = "ApplyQueryOptionsAsync: Should apply all filter, sort, search and page criteria correctly")]    public async Task ApplyQueryOptionsAsync_AppliesAllCriteria()
    {
        var baseName = $"Opt_{Guid.NewGuid()}";
        // Create 3 items
        // 1. Name=Opt_..._Apple, Price=10
        // 2. Name=Opt_..._Banana, Price=20
        // 3. Name=Opt_..._Apple, Price=30

        _fixture.Context.Set<Example>().AddRange(
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Apple", Price = 10 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Banana", Price = 20 },
            new Example { Id = Guid.NewGuid(), Name = $"{baseName}_Apple", Price = 30 }
        );
        await _fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = _fixture.Context.Set<Example>();

        var builder = new QueryBuilder<Example>();
        builder
            .Where(x => x.Name, "*", baseName) // Filter by base name
            .Where(x => x.Name, "*", "Apple")  // Filter by Apple
            .OrderByDescending(x => x.Price)   // Sort by Price Desc
            .Page(1, 1);                       // Take 1st page of size 1

        var options = builder.Build();

        var result = await query.ApplyQueryOptionsAsync(options, TestContext.Current.CancellationToken);

        // Expected matches: Apple(10), Apple(30).
        // Sorted Desc: Apple(30), Apple(10).
        // Page 1, Size 1: Apple(30).

        result.TotalCount.Should().Be(2); // Total matches after filter
        result.Items.Should().HaveCount(1);
        result.Items[0].Price.Should().Be(30);
    }
}
