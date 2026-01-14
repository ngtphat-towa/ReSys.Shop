using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Query")]
public class FilterExtensionsTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public ExampleStatus Status { get; set; }
        public Guid ExternalId { get; set; }
        public Address? Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; } = string.Empty;
        public ZipCode? Zip { get; set; }
    }

    public class ZipCode
    {
        public string Code { get; set; } = string.Empty;
    }

    private readonly IQueryable<TestItem> _data;

    public FilterExtensionsTests()
    {
        _data = new List<TestItem>
        {
            new() { Id = 1, Name = "Apple", Price = 10.0, CreatedAt = new DateTime(2023, 1, 1), IsActive = true, Status = ExampleStatus.Active, ExternalId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Address = new Address { City = "Cupertino", Zip = new ZipCode { Code = "95014" } } },
            new() { Id = 2, Name = "Banana", Price = 20.0, CreatedAt = new DateTime(2023, 1, 2), IsActive = false, Status = ExampleStatus.Archived, ExternalId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Address = new Address { City = "Hanoi" } },
            new() { Id = 3, Name = "Orange", Price = 30.0, CreatedAt = new DateTime(2023, 1, 3), IsActive = true, Status = ExampleStatus.Active, ExternalId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Address = null },
            new() { Id = 4, Name = "Pear", Price = 15.0, CreatedAt = new DateTime(2023, 1, 4), IsActive = true, Status = ExampleStatus.Draft, ExternalId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Address = new Address { City = "London" } },
            new() { Id = 5, Name = "Pineapple", Price = 50.0, CreatedAt = new DateTime(2023, 1, 5), IsActive = false, Status = ExampleStatus.Active, ExternalId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Address = new Address { City = "Paris" } },
            new() { Id = 6, Name = "Apricot", Price = 10.0, CreatedAt = DateTime.Now, IsActive = true, Status = ExampleStatus.Draft }
        }.AsQueryable();
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for simple equals")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForSimpleEquals()
    {
        var result = _data.ApplyDynamicFilter("Price=20").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should exclude matching items for not equal")]
    public void ApplyDynamicFilter_Should_ExcludeMatchingItems_ForNotEqual()
    {
        var result = _data.ApplyDynamicFilter("Id!=1").ToList();
        result.Should().HaveCount(5);
        result.Should().NotContain(x => x.Id == 1);
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for greater than")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForGreaterThan()
    {
        var result = _data.ApplyDynamicFilter("Price>20").ToList();
        result.Should().HaveCount(2); 
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for greater than or equal")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForGreaterThanOrEqual()
    {
        var result = _data.ApplyDynamicFilter("Price>=30").ToList();
        result.Should().HaveCount(2); // Orange (30), Pineapple (50)
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for less than")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForLessThan()
    {
        var result = _data.ApplyDynamicFilter("Price<15").ToList();
        result.Should().HaveCount(2); // Apple (10), Apricot (10)
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for less than or equal")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForLessThanOrEqual()
    {
        var result = _data.ApplyDynamicFilter("Price<=15").ToList();
        result.Should().HaveCount(3); // Apple (10), Pear (15), Apricot (10)
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for contains wildcard")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForContainsWildcard()
    {
        var result = _data.ApplyDynamicFilter("name=*ap*").ToList();
        // Apple, Pineapple, Apricot
        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Name == "Apple");
        result.Should().Contain(x => x.Name == "Pineapple");
        result.Should().Contain(x => x.Name == "Apricot");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return non-matching items for not contains")]
    public void ApplyDynamicFilter_Should_ReturnNonMatchingItems_ForNotContains()
    {
        var result = _data.ApplyDynamicFilter("name!*ap").ToList();
        // Banana, Orange, Pear
        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Name == "Banana");
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pear");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for starts with")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForStartsWith()
    {
        var result = _data.ApplyDynamicFilter("Name^P").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Pear");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for ends with")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForEndsWith()
    {
        var result = _data.ApplyDynamicFilter("Name$e").ToList();
        // Apple, Orange, Pineapple
        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should filter by enum correctly")]
    public void ApplyDynamicFilter_Should_FilterByEnum_Correctly()
    {
        var result = _data.ApplyDynamicFilter("Status=Archived").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }
    
    [Fact(DisplayName = "ApplyDynamicFilter: Should filter by Guid correctly")]
    public void ApplyDynamicFilter_Should_FilterByGuid_Correctly()
    {
        var result = _data.ApplyDynamicFilter("ExternalId=11111111-1111-1111-1111-111111111111").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should filter by Date correctly")]
    public void ApplyDynamicFilter_Should_FilterByDate_Correctly()
    {
        var result = _data.ApplyDynamicFilter("CreatedAt=2023-01-02").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should filter by nested property correctly")]
    public void ApplyDynamicFilter_Should_FilterByNestedProperty_Correctly()
    {
        var result = _data.ApplyDynamicFilter("Address.City=London").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Pear");
    }
    
    [Fact(DisplayName = "ApplyDynamicFilter: Should filter by deep nested property correctly")]
    public void ApplyDynamicFilter_Should_FilterByDeepNestedProperty_Correctly()
    {
        var result = _data.ApplyDynamicFilter("Address.Zip.Code=95014").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should not crash when nested property is null")]
    public void ApplyDynamicFilter_Should_NotCrash_WhenNestedPropertyIsNull()
    {
        // Id=3 has null Address. 
        var result = _data.ApplyDynamicFilter("Address.City=London").ToList();
        result.Should().HaveCount(1);
    }
    
    [Fact(DisplayName = "ApplyDynamicFilter: Should not crash when intermediate deep nested property is null")]
    public void ApplyDynamicFilter_Should_NotCrash_WhenIntermediateDeepNestedPropertyIsNull()
    {
        // Banana has Address but no Zip (null).
        // Orange has null Address.
        var result = _data.ApplyDynamicFilter("Address.Zip.Code=95014").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should work with case-insensitive property names")]
    public void ApplyDynamicFilter_Should_WorkWithCaseInsensitivePropertyNames()
    {
        var result = _data.ApplyDynamicFilter("name=Apple").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should work with snake_case property names")]
    public void ApplyDynamicFilter_Should_WorkWithSnakeCasePropertyNames()
    {
        var result = _data.ApplyDynamicFilter("created_at=2023-01-01").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should handle logical AND correctly")]
    public void ApplyDynamicFilter_Should_HandleLogicalAnd_Correctly()
    {
        var result = _data.ApplyDynamicFilter("Price>10,IsActive=true").ToList();
        result.Should().HaveCount(2); // Orange, Pear
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should handle logical OR correctly")]
    public void ApplyDynamicFilter_Should_HandleLogicalOr_Correctly()
    {
        var result = _data.ApplyDynamicFilter("Name=Apple|Name=Banana").ToList();
        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should handle grouping with precedence correctly")]
    public void ApplyDynamicFilter_Should_HandleGroupingWithPrecedence_Correctly()
    {
        // (Price > 40 OR Price < 15) AND IsActive = true
        var result = _data.ApplyDynamicFilter("(Price>40|Price<15),IsActive=true").ToList();
        result.Should().HaveCount(2); // Apple, Apricot
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should ignore invalid fields")]
    public void ApplyDynamicFilter_Should_IgnoreInvalidFields()
    {
        var result = _data.ApplyDynamicFilter("InvalidField=10").ToList();
        result.Should().HaveCount(6);
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return all items for empty filter string")]
    public void ApplyDynamicFilter_Should_ReturnAllItems_ForEmptyFilterString()
    {
        var result = _data.ApplyDynamicFilter("").ToList();
        result.Should().HaveCount(6);
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for equals null")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForEqualsNull()
    {
        // Orange and Apricot have null Address
        var result = _data.ApplyDynamicFilter("Address=null").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Apricot");
    }

    [Fact(DisplayName = "ApplyDynamicFilter: Should return matching items for not equals null")]
    public void ApplyDynamicFilter_Should_ReturnMatchingItems_ForNotEqualsNull()
    {
        // Apple, Banana, Pear, Pineapple have Address (4 items)
        var result = _data.ApplyDynamicFilter("Address!=null").ToList();
        result.Should().HaveCount(4);
    }
}
