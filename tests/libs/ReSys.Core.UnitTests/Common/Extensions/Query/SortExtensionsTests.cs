using ReSys.Core.Common.Extensions.Sort;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Query")]
public class SortExtensionsTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public Address? Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; } = string.Empty;
    }

    private readonly IQueryable<TestItem> _data;

    public SortExtensionsTests()
    {
        _data = new List<TestItem>
        {
            new() { Name = "Apple", Price = 10.0, CreatedAt = new DateTime(2023, 1, 1), Address = new Address { City = "Cupertino" } },
            new() { Name = "Banana", Price = 20.0, CreatedAt = new DateTime(2023, 1, 2), Address = new Address { City = "Hanoi" } },
            new() { Name = "Orange", Price = 30.0, CreatedAt = new DateTime(2023, 1, 3), Address = null },
            new() { Name = "Pear", Price = 15.0, CreatedAt = new DateTime(2023, 1, 4), Address = new Address { City = "London" } },
            new() { Name = "Pineapple", Price = 50.0, CreatedAt = new DateTime(2023, 1, 5), Address = new Address { City = "Paris" } },
            new() { Name = "Apricot", Price = 10.0, CreatedAt = DateTime.Now }
        }.AsQueryable();
    }

    [Fact(DisplayName = "ApplyDynamicSort: Should return items in correct order for ascending sort")]
    public void ApplyDynamicSort_Should_ReturnSorted_WhenAscending()
    {
        var result = _data.ApplyDynamicSort("Price", isDescending: false).ToList();
        result.First().Price.Should().Be(10);
        result.Last().Price.Should().Be(50);
    }

    [Fact(DisplayName = "ApplyDynamicSort: Should return items in correct order for descending sort")]
    public void ApplyDynamicSort_Should_ReturnSorted_WhenDescending()
    {
        var result = _data.ApplyDynamicSort("Price", isDescending: true).ToList();
        result.First().Price.Should().Be(50);
        result.Last().Price.Should().Be(10);
    }

    [Fact(DisplayName = "ApplyDynamicSort: Should work correctly with snake_case property names")]
    public void ApplyDynamicSort_Should_ReturnSorted_WhenSnakeCase()
    {
        var result = _data.ApplyDynamicSort("created_at", isDescending: false).ToList();
        result.First().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicSort: Should work correctly with nested property sorting")]
    public void ApplyDynamicSort_Should_ReturnSorted_WhenNestedProperty()
    {
        var result = _data.ApplyDynamicSort("Address.City", isDescending: false).ToList();
        var withAddress = result.Where(x => x.Address != null).ToList();
        // C, H, L, P
        withAddress[0].Address!.City.Should().Be("Cupertino");
        withAddress[3].Address!.City.Should().Be("Paris");
    }

    [Fact(DisplayName = "ApplyDynamicOrdering: Should work correctly with multiple field ordering")]
    public void ApplyDynamicOrdering_Should_WorkCorrectly_WithMultipleFields()
    {
        // Sort by Price DESC, then Name ASC
        var result = _data.ApplyDynamicOrdering("Price desc, Name").ToList();

        result[0].Name.Should().Be("Pineapple"); // 50
        result[4].Name.Should().Be("Apple");     // 10
        result[5].Name.Should().Be("Apricot");   // 10
    }

    [Fact(DisplayName = "ApplyDynamicOrdering: Should work correctly with explicit 'asc' suffix")]
    public void ApplyDynamicOrdering_Should_ReturnSorted_WithExplicitAsc()
    {
        var result = _data.ApplyDynamicOrdering("Price asc").ToList();
        result.First().Price.Should().Be(10);
    }

    [Fact(DisplayName = "ApplyDynamicOrdering: Should ignore invalid fields and sort by valid ones")]
    public void ApplyDynamicOrdering_Should_IgnoreInvalidField()
    {
        var result = _data.ApplyDynamicOrdering("InvalidField, Name desc").ToList();
        // Should sort by Name desc only
        result.First().Name.Should().Be("Pineapple");
    }

    [Fact(DisplayName = "ApplyDynamicOrdering: Should return original order when sort string is empty")]
    public void ApplyDynamicOrdering_Should_ReturnOriginal_WhenEmptyString()
    {
        var list = _data.ToList();
        var result = _data.ApplyDynamicOrdering("").ToList();
        result.Should().BeEquivalentTo(list);
    }
}
