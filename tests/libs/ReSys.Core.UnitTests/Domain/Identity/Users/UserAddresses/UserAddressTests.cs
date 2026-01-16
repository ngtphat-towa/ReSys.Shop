using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.UnitTests.Domain.Identity.Users.UserAddresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class UserAddressTests
{
    [Fact(DisplayName = "Create should fail if Address is invalid")]
    public void Create_ShouldFail_IfAddressInvalid()
    {
        // Arrange
        // Address.Create checks validation
        var addressResult = Address.Create("", "City", "12345", "US");

        // Assert
        addressResult.IsError.Should().BeTrue();
        addressResult.FirstError.Should().Be(AddressErrors.Address1Required);
    }

    [Fact(DisplayName = "Create should fail if Label is too long")]
    public void Create_ShouldFail_IfLabelTooLong()
    {
        // Arrange
        var address = Address.Create("123 St", "City", "12345", "US").Value;
        var longLabel = new string('a', UserAddressConstraints.LabelMaxLength + 1);

        // Act
        var result = UserAddress.Create("user1", address, longLabel);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserAddressErrors.LabelTooLong);
    }

    [Fact(DisplayName = "Create should succeed with valid data")]
    public void Create_ShouldSucceed_WithValidData()
    {
        // Arrange
        var address = Address.Create("123 St", "City", "12345", "US").Value;
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = UserAddress.Create(userId, address, "Home", AddressType.Billing);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Address.Address1.Should().Be("123 St");
        result.Value.Label.Should().Be("Home");
        result.Value.Type.Should().Be(AddressType.Billing);
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "MarkAsDefault and UnmarkAsDefault should update state")]
    public void Default_State_ShouldUpdate()
    {
        // Arrange
        var address = Address.Create("123 St", "City", "12345", "US").Value;
        var userAddress = UserAddress.Create("user1", address).Value;

        // Act & Assert 1
        userAddress.MarkAsDefault();
        userAddress.IsDefault.Should().BeTrue();

        // Act & Assert 2
        userAddress.UnmarkAsDefault();
        userAddress.IsDefault.Should().BeFalse();
    }
    
    [Fact(DisplayName = "Verify should set IsVerified to true")]
    public void Verify_ShouldSet_IsVerified()
    {
        // Arrange
        var address = Address.Create("123 St", "City", "12345", "US").Value;
        var userAddress = UserAddress.Create("user1", address).Value;

        // Act
        userAddress.Verify();

        // Assert
        userAddress.IsVerified.Should().BeTrue();
    }
}
