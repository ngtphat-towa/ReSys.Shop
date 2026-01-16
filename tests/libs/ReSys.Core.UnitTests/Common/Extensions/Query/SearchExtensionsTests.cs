using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Search;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Query")]
public class SearchExtensionsTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public Address? Address { get; set; }
        public int Age { get; set; }
    }

    public class Address
    {
        public string City { get; set; } = string.Empty;
    }

    private readonly IQueryable<TestItem> _data;

    public SearchExtensionsTests()
    {
        _data = new List<TestItem>
        {
            new() { Name = "Apple", Address = new Address { City = "Cupertino" } },
            new() { Name = "Banana", Address = new Address { City = "Hanoi" } },
            new() { Name = "Orange", Address = null },
            new() { Name = "Pear", Address = new Address { City = "London" } },
            new() { Name = "Pineapple", Address = new Address { City = "Paris" } }
        }.AsQueryable();
    }

    [Fact(DisplayName = "ApplySearch: Global search should match any provided field")]
    public void ApplySearch_Should_MatchAnyField_WhenValidQueryProvided()
    {
        // Search "apple" in Name or Address.City
        var result = _data.ApplySearch("apple", "Name", "Address.City").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Apple");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "ApplySearch: Global search should match nested properties")]
    public void ApplySearch_Should_MatchNestedField_WhenNestedPropertyTargeted()
    {
        // Search "don" -> London (Address.City)
        var result = _data.ApplySearch("don", "Name", "Address.City").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Pear");
    }

    [Fact(DisplayName = "ApplySearch: Should return empty if no matches found")]
    public void ApplySearch_Should_ReturnEmpty_WhenNoMatchesFound()
    {
        var result = _data.ApplySearch("xyz", "Name").ToList();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "ApplySearch: Should return all items for empty search text")]
    public void ApplySearch_Should_ReturnAllItems_WhenSearchTextIsEmpty()
    {
        var result = _data.ApplySearch("", "Name").ToList();
        result.Should().HaveCount(5);
    }

    [Fact(DisplayName = "ApplySearch: Should return all items when search fields are null")]
    public void ApplySearch_Should_ReturnAllItems_WhenFieldsAreNull()
    {
        var result = _data.ApplySearch("apple", null).ToList();
        result.Should().HaveCount(5);
    }

    [Fact(DisplayName = "ApplySearch: Should work correctly with non-string fields")]
    public void ApplySearch_Should_SearchNonStringFields_WhenProvided()
    {
        // Age is int, should be searchable via ToString()
        var dataWithAge = new List<TestItem>
        {
            new() { Name = "Young", Age = 10 },
            new() { Name = "Old", Age = 99 }
        }.AsQueryable();

        var result = dataWithAge.ApplySearch("99", "Age").ToList();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Old");
    }
}
