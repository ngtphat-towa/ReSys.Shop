namespace ReSys.Core.Domain.Location.Addresses;

public static class AddressConstraints
{
    public const int Address1MaxLength = 100;
    public const int Address2MaxLength = 100;
    public const int CityMaxLength = 50;
    public const int ZipCodeMaxLength = 20;
    public const int CountryCodeLength = 2; // ISO 3166-1 alpha-2
    public const int StateCodeMaxLength = 10;
    public const int NameMaxLength = 100;
    public const int PhoneMaxLength = 20;
}
