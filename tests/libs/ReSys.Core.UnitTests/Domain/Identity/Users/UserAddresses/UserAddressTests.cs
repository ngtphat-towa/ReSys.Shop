using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.UserAddresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "UserAddress")]
public class UserAddressTests
{
    private readonly Address _validAddress = Address.Create("123 Main St", "City", "12345", "US").Value;
    private readonly string _userId = Guid.NewGuid().ToString();

    [Fact(DisplayName = "Create: Should successfully initialize address")]
    public void Create_Should_InitializeAddress()
    {
        // Act
        var result = UserAddress.Create(_userId, _validAddress, "Home", AddressType.Shipping, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(_userId);
        result.Value.Address.Should().Be(_validAddress);
        result.Value.Label.Should().Be("Home");
        result.Value.Type.Should().Be(AddressType.Shipping);
        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Create: Should fail if label exceeds max length")]
    public void Create_ShouldFail_IfLabelTooLong()
    {
        // Arrange
        var longLabel = new string('A', UserAddressConstraints.LabelMaxLength + 1);

        // Act
        var result = UserAddress.Create(_userId, _validAddress, longLabel);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserAddressErrors.LabelTooLong);
    }

    [Fact(DisplayName = "Update: Should change properties")]
    public void Update_Should_ChangeProperties()
    {
        // Arrange
        var userAddress = UserAddress.Create(_userId, _validAddress, "Home").Value;
        var newAddress = Address.Create("456 Oak St", "Other City", "54321", "US").Value;

        // Act
        var result = userAddress.Update("Work", newAddress, AddressType.Billing);

        // Assert
        result.IsError.Should().BeFalse();
        userAddress.Label.Should().Be("Work");
        userAddress.Address.Should().Be(newAddress);
        userAddress.Type.Should().Be(AddressType.Billing);
    }

    [Fact(DisplayName = "MarkAsDefault: Should set flag")]
    public void MarkAsDefault_Should_SetFlag()
    {
        // Arrange
        var userAddress = UserAddress.Create(_userId, _validAddress).Value;

        // Act
        userAddress.MarkAsDefault();

        // Assert
        userAddress.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Verify: Should set flag")]
    public void Verify_Should_SetFlag()
    {
        // Arrange
        var userAddress = UserAddress.Create(_userId, _validAddress).Value;

        // Act
        userAddress.Verify();

        // Assert
        userAddress.IsVerified.Should().BeTrue();
    }
}
