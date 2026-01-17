using ReSys.Shared.Models;
using ReSys.Shared.Models.Filters;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Search;

namespace ReSys.Shared.UnitTests.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class QueryOptionsTests
{
    [Fact(DisplayName = "FilterOptions: Should store filter string correctly")]
    public void FilterOptions_ShouldStoreFilterString()
    {
        var options = new FilterOptions { Filter = "Name=Test" };
        options.Filter.Should().Be("Name=Test");
    }

    [Fact(DisplayName = "SortOptions: Should store sort string correctly")]
    public void SortOptions_ShouldStoreSortString()
    {
        var options = new SortOptions { Sort = "Name desc" };
        options.Sort.Should().Be("Name desc");
    }

    [Fact(DisplayName = "SearchOptions: Should store search text and fields correctly")]
    public void SearchOptions_ShouldStoreSearchTextAndFields()
    {
        var options = new SearchOptions
        {
            Search = "test",
            SearchField = new[] { "Name", "Description" }
        };
        options.Search.Should().Be("test");
        options.SearchField.Should().HaveCount(2).And.Contain("Name").And.Contain("Description");
    }

    [Fact(DisplayName = "PageOptions: Should store page and page size correctly")]
    public void PageOptions_ShouldStorePageAndPageSize()
    {
        var options = new PageOptions { Page = 2, PageSize = 50 };
        options.Page.Should().Be(2);
        options.PageSize.Should().Be(50);
    }

    [Fact(DisplayName = "QueryOptions: Should implement all query interfaces")]
    public void QueryOptions_ShouldImplementAllInterfaces()
    {
        var options = new QueryOptions
        {
            Filter = "Name=Test",
            Sort = "Name",
            Search = "test",
            SearchField = new[] { "Name" },
            Page = 1,
            PageSize = 10
        };

        options.Should().BeAssignableTo<IFilterOptions>();
        options.Should().BeAssignableTo<ISortOptions>();
        options.Should().BeAssignableTo<ISearchOptions>();
        options.Should().BeAssignableTo<IPageOptions>();

        options.Filter.Should().Be("Name=Test");
        options.Sort.Should().Be("Name");
        options.Search.Should().Be("test");
        options.Page.Should().Be(1);
        options.PageSize.Should().Be(10);
    }
}
