using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

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

    [Fact(DisplayName = "Search: Global search should match any provided field")]
    public void ApplySearch_MatchesAnyField()
    {
        // Search "apple" in Name or Address.City
        var result = _data.ApplySearch("apple", "Name", "Address.City").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Apple");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "Search: Global search should match nested properties")]
    public void ApplySearch_MatchesNestedField()
    {
        // Search "don" -> London (Address.City)
        var result = _data.ApplySearch("don", "Name", "Address.City").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Pear");
    }

    [Fact(DisplayName = "Search: Should return empty if no matches found")]
    public void ApplySearch_NoMatches_ReturnsEmpty()
    {
        var result = _data.ApplySearch("xyz", "Name").ToList();
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "Search: Empty search text should return all items")]
    public void ApplySearch_EmptySearchText_ReturnsAll()
    {
        var result = _data.ApplySearch("", "Name").ToList();
        result.Should().HaveCount(5);
    }

    [Fact(DisplayName = "Search: Null search fields should return all items")]
    public void ApplySearch_NullFields_ReturnsAll()
    {
        var result = _data.ApplySearch("apple", null).ToList();
        result.Should().HaveCount(5);
    }

    [Fact(DisplayName = "Search: Non-string fields should be searched")]
    public void ApplySearch_NonStringField_Searched()
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
