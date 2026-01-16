using ErrorOr;

namespace ReSys.Core.Domain.Location.Addresses;

public static class AddressErrors
{
    public static Error Address1Required => Error.Validation(
        code: "Address.Address1Required",
        description: "Address Line 1 is required.");

    public static Error CityRequired => Error.Validation(
        code: "Address.CityRequired",
        description: "City is required.");

    public static Error ZipCodeRequired => Error.Validation(
        code: "Address.ZipCodeRequired",
        description: "Zip/Postal Code is required.");

    public static Error CountryCodeRequired => Error.Validation(
        code: "Address.CountryCodeRequired",
        description: "Country Code is required.");

    public static Error InvalidCountryCode => Error.Validation(
        code: "Address.InvalidCountryCode",
        description: $"Country Code must be {AddressConstraints.CountryCodeLength} characters (ISO 3166-1 alpha-2).");
}
