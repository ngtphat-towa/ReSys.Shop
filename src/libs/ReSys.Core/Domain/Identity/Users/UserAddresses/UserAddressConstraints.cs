using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.Domain.Identity.Users.UserAddresses;

public static class UserAddressConstraints
{
    public const int LabelMaxLength = 50; // e.g. "Work", "Home"
    
    // Re-expose useful constraints from the base Address model for convenience
    public const int NameMaxLength = AddressConstraints.NameMaxLength;
}
