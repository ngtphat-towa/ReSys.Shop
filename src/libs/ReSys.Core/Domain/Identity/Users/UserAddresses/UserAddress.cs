using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Location.Addresses;

using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.UserAddresses;

public sealed class UserAddress : Aggregate, IHasMetadata
{
    public string UserId { get; set; } = string.Empty;
    public string Label { get; set; } = "Default";
    public AddressType Type { get; set; } = AddressType.Both;
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }

    // Immutable Value Object
    public Address Address { get; set; } = null!;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public UserAddress() { }

    public static ErrorOr<UserAddress> Create(
        string userId,
        Address address,
        string label = "Default",
        AddressType type = AddressType.Both,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Error.Validation("UserAddress.UserIdRequired", "User ID is required.");
        if (address == null) return UserAddressErrors.AddressRequired;
        if (string.IsNullOrWhiteSpace(label)) return UserAddressErrors.LabelRequired;
        if (label.Length > UserAddressConstraints.LabelMaxLength) return UserAddressErrors.LabelTooLong;

        return new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Address = address,
            Label = label.Trim(),
            Type = type,
            IsDefault = isDefault
        };
    }

    public ErrorOr<Success> Update(string label, Address address, AddressType type)
    {
        if (address == null) return UserAddressErrors.AddressRequired;
        if (string.IsNullOrWhiteSpace(label)) return UserAddressErrors.LabelRequired;
        if (label.Length > UserAddressConstraints.LabelMaxLength) return UserAddressErrors.LabelTooLong;

        Label = label.Trim();
        Address = address;
        Type = type;

        return Result.Success;
    }

    public void MarkAsDefault() => IsDefault = true;
    public void UnmarkAsDefault() => IsDefault = false;
    public void Verify() => IsVerified = true;

    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        PublicMetadata = publicMetadata;
        PrivateMetadata = privateMetadata;
    }
}

public enum AddressType
{
    Shipping,
    Billing,
    Both
}
