using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Location.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Location")]
[Trait("Domain", "Address")]
public class AddressTests
{
    [Fact(DisplayName = "Create: Should successfully initialize address")]
    public void Create_Should_InitializeAddress()
    {
        // Act
        var result = Address.Create("123 Main St", "Prague", "11000", "CZ", "John", "Doe");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Address1.Should().Be("123 Main St");
        result.Value.City.Should().Be("Prague");
        result.Value.ZipCode.Should().Be("11000");
        result.Value.CountryCode.Should().Be("CZ");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.FullName.Should().Be("John Doe");
    }

    [Fact(DisplayName = "Create: Should fail if address1 is missing")]
    public void Create_ShouldFail_IfAddress1Missing()
    {
        // Act
        var result = Address.Create("", "City", "12345", "US");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AddressErrors.Address1Required);
    }

    [Fact(DisplayName = "Create: Should fail if country code is not 2 chars")]
    public void Create_ShouldFail_IfInvalidCountryCode()
    {
        // Act
        var result = Address.Create("Street", "City", "12345", "USA");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AddressErrors.InvalidCountryCode);
    }

    [Fact(DisplayName = "Update: Should change properties")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var address = Address.Create("Old Street", "Old City", "11111", "CZ").Value;

        // Act
        var result = address.Update("New Street", "New City", "22222", "US", "Jane", "Smith");

        // Assert
        result.IsError.Should().BeFalse();
        address.Address1.Should().Be("New Street");
        address.City.Should().Be("New City");
        address.ZipCode.Should().Be("22222");
        address.CountryCode.Should().Be("US");
        address.FullName.Should().Be("Jane Smith");
    }

    [Fact(DisplayName = "Equality: Should be equal when properties match")]
    public void Equality_Should_WorkByValue()
    {
        // Arrange
        var addr1 = Address.Create("Street", "City", "12345", "US").Value;
        var addr2 = Address.Create("Street", "City", "12345", "US").Value;
        var addr3 = Address.Create("Other", "City", "12345", "US").Value;

        // Assert
        (addr1 == addr2).Should().BeTrue();
        addr1.Equals(addr2).Should().BeTrue();
        (addr1 == addr3).Should().BeFalse();
        addr1.GetHashCode().Should().Be(addr2.GetHashCode());
    }

    [Fact(DisplayName = "ToString: Should format address correctly")]
    public void ToString_Should_FormatCorrectly()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Prague", "11000", "CZ", "John", "Doe", company: "ReSys").Value;

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Contain("John Doe");
        result.Should().Contain("ReSys");
        result.Should().Contain("123 Main St");
        result.Should().Contain("Prague");
        result.Should().Contain("11000");
        result.Should().Contain("CZ");
    }
}