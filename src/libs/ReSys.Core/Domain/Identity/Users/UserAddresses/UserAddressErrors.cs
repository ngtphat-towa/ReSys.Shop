using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.UserAddresses;

public static class UserAddressErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "UserAddress.NotFound",
        description: $"Address with ID '{id}' was not found.");

    public static Error AddressRequired => Error.Validation(
        code: "UserAddress.AddressRequired",
        description: "Address details are required.");

    public static Error LabelTooLong => Error.Validation(
        code: "UserAddress.LabelTooLong",
        description: $"Address label cannot exceed {UserAddressConstraints.LabelMaxLength} characters.");
}
