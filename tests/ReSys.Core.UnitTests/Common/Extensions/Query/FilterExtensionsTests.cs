using FluentAssertions;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Domain;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Extensions.Query;

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

    [Fact(DisplayName = "Filter: Simple Equals should return matching items")]
    public void ApplyDynamicFilter_SimpleEquals_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price=20").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }

    [Fact(DisplayName = "Filter: Not Equal should exclude matching items")]
    public void ApplyDynamicFilter_NotEqual_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Id!=1").ToList();
        result.Should().HaveCount(5);
        result.Should().NotContain(x => x.Id == 1);
    }

    [Fact(DisplayName = "Filter: Greater Than should return matching items")]
    public void ApplyDynamicFilter_GreaterThan_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price>20").ToList();
        result.Should().HaveCount(2); 
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "Filter: Greater Than or Equal should return matching items")]
    public void ApplyDynamicFilter_GreaterThanOrEqual_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price>=30").ToList();
        result.Should().HaveCount(2); // Orange (30), Pineapple (50)
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "Filter: Less Than should return matching items")]
    public void ApplyDynamicFilter_LessThan_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price<15").ToList();
        result.Should().HaveCount(2); // Apple (10), Apricot (10)
    }

    [Fact(DisplayName = "Filter: Less Than or Equal should return matching items")]
    public void ApplyDynamicFilter_LessThanOrEqual_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price<=15").ToList();
        result.Should().HaveCount(3); // Apple (10), Pear (15), Apricot (10)
    }

    [Fact(DisplayName = "Filter: Contains wildcard should return matching items")]
    public void ApplyDynamicFilter_ContainsWildcard_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("name=*ap*").ToList();
        // Apple, Pineapple, Apricot
        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Name == "Apple");
        result.Should().Contain(x => x.Name == "Pineapple");
        result.Should().Contain(x => x.Name == "Apricot");
    }

    [Fact(DisplayName = "Filter: Not Contains should return non-matching items")]
    public void ApplyDynamicFilter_NotContains_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("name!*ap").ToList();
        // Banana, Orange, Pear
        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Name == "Banana");
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Pear");
    }

    [Fact(DisplayName = "Filter: StartsWith should return matching items")]
    public void ApplyDynamicFilter_StartsWith_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Name^P").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Pear");
        result.Should().Contain(x => x.Name == "Pineapple");
    }

    [Fact(DisplayName = "Filter: EndsWith should return matching items")]
    public void ApplyDynamicFilter_EndsWith_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Name$e").ToList();
        // Apple, Orange, Pineapple
        result.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Filter: Enum filtering should work")]
    public void ApplyDynamicFilter_Enum_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Status=Archived").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }
    
    [Fact(DisplayName = "Filter: Guid filtering should work")]
    public void ApplyDynamicFilter_Guid_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("ExternalId=11111111-1111-1111-1111-111111111111").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Filter: Date filtering should work")]
    public void ApplyDynamicFilter_Date_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("CreatedAt=2023-01-02").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Banana");
    }

    [Fact(DisplayName = "Filter: Nested property filtering should work")]
    public void ApplyDynamicFilter_NestedProperty_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Address.City=London").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Pear");
    }
    
    [Fact(DisplayName = "Filter: Deep nested property filtering should work")]
    public void ApplyDynamicFilter_DeepNestedProperty_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Address.Zip.Code=95014").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Filter: Nested property with null should not crash")]
    public void ApplyDynamicFilter_NestedProperty_WithNull_DoesNotCrash()
    {
        // Id=3 has null Address. 
        var result = _data.ApplyDynamicFilter("Address.City=London").ToList();
        result.Should().HaveCount(1);
    }
    
    [Fact(DisplayName = "Filter: Deep nested property with intermediate null should not crash")]
    public void ApplyDynamicFilter_DeepNestedProperty_WithIntermediateNull_DoesNotCrash()
    {
        // Banana has Address but no Zip (null).
        // Orange has null Address.
        var result = _data.ApplyDynamicFilter("Address.Zip.Code=95014").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Filter: Case-insensitive property name should work")]
    public void ApplyDynamicFilter_CaseInsensitiveProperty_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("name=Apple").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Filter: Snake case property name should work")]
    public void ApplyDynamicFilter_SnakeCaseProperty_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("created_at=2023-01-01").ToList();
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Apple");
    }

    [Fact(DisplayName = "Filter: Logical AND should work")]
    public void ApplyDynamicFilter_LogicalAnd_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Price>10,IsActive=true").ToList();
        result.Should().HaveCount(2); // Orange, Pear
    }

    [Fact(DisplayName = "Filter: Logical OR should work")]
    public void ApplyDynamicFilter_LogicalOr_ReturnsCorrectItems()
    {
        var result = _data.ApplyDynamicFilter("Name=Apple|Name=Banana").ToList();
        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Filter: Grouping with precedence should work")]
    public void ApplyDynamicFilter_GroupingWithPrecedence_ReturnsCorrectItems()
    {
        // (Price > 40 OR Price < 15) AND IsActive = true
        var result = _data.ApplyDynamicFilter("(Price>40|Price<15),IsActive=true").ToList();
        result.Should().HaveCount(2); // Apple, Apricot
    }

    [Fact(DisplayName = "Filter: Invalid field should be ignored")]
    public void ApplyDynamicFilter_InvalidField_Ignored()
    {
        var result = _data.ApplyDynamicFilter("InvalidField=10").ToList();
        result.Should().HaveCount(6);
    }

    [Fact(DisplayName = "Filter: Empty string should return all items")]
    public void ApplyDynamicFilter_EmptyString_ReturnsAll()
    {
        var result = _data.ApplyDynamicFilter("").ToList();
        result.Should().HaveCount(6);
    }

    [Fact(DisplayName = "Filter: Equals null should return matching items")]
    public void ApplyDynamicFilter_EqualsNull_ReturnsCorrectItems()
    {
        // Orange and Apricot have null Address
        var result = _data.ApplyDynamicFilter("Address=null").ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Orange");
        result.Should().Contain(x => x.Name == "Apricot");
    }

    [Fact(DisplayName = "Filter: Not Equals null should return matching items")]
    public void ApplyDynamicFilter_NotEqualsNull_ReturnsCorrectItems()
    {
        // Apple, Banana, Pear, Pineapple have Address (4 items)
        var result = _data.ApplyDynamicFilter("Address!=null").ToList();
        result.Should().HaveCount(4);
    }
}
