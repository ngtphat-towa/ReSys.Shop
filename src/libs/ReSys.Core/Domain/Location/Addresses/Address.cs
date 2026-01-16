using ErrorOr;

namespace ReSys.Core.Domain.Location.Addresses;

/// <summary>
/// Represents a physical address.
/// Rich POCO: Mutable for ease of use, but with factory for validation.
/// </summary>
public class Address
{
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Identity
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }

    // Location Data (Internal Links)
    public Guid? CountryId { get; set; }
    public Guid? StateId { get; set; }

    // Location Data (ISO Standards)
    public string? CountryCode { get; set; } // e.g., "US", "CZ"
    public string? StateCode { get; set; }   // e.g., "CA", "NY"

    public string FullName => $"{FirstName} {LastName}".Trim();

    public Address() { }

    // Factory method for consistent creation
    public static ErrorOr<Address> Create(
        string address1, 
        string city, 
        string zipCode, 
        string countryCode,
        string? firstName = null, 
        string? lastName = null,
        Guid? countryId = null,
        string? address2 = null,
        string? phone = null,
        string? company = null,
        Guid? stateId = null,
        string? stateCode = null)
    {
        if (string.IsNullOrWhiteSpace(address1)) return AddressErrors.Address1Required;
        if (string.IsNullOrWhiteSpace(city)) return AddressErrors.CityRequired;
        if (string.IsNullOrWhiteSpace(zipCode)) return AddressErrors.ZipCodeRequired;
        if (string.IsNullOrWhiteSpace(countryCode)) return AddressErrors.CountryCodeRequired;
        if (countryCode.Length != AddressConstraints.CountryCodeLength) return AddressErrors.InvalidCountryCode;

        return new Address
        {
            Address1 = address1.Trim(),
            Address2 = address2?.Trim(),
            City = city.Trim(),
            ZipCode = zipCode.Trim(),
            CountryCode = countryCode.ToUpperInvariant(),
            CountryId = countryId,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            Phone = phone?.Trim(),
            Company = company?.Trim(),
            StateId = stateId,
            StateCode = stateCode?.ToUpperInvariant()
        };
    }

    public ErrorOr<Success> Update(
        string address1, 
        string city, 
        string zipCode, 
        string countryCode,
        string? firstName = null, 
        string? lastName = null,
        Guid? countryId = null,
        string? address2 = null,
        string? phone = null,
        string? company = null,
        Guid? stateId = null,
        string? stateCode = null)
    {
        if (string.IsNullOrWhiteSpace(address1)) return AddressErrors.Address1Required;
        if (string.IsNullOrWhiteSpace(city)) return AddressErrors.CityRequired;
        if (string.IsNullOrWhiteSpace(zipCode)) return AddressErrors.ZipCodeRequired;
        if (string.IsNullOrWhiteSpace(countryCode)) return AddressErrors.CountryCodeRequired;
        if (countryCode.Length != AddressConstraints.CountryCodeLength) return AddressErrors.InvalidCountryCode;

        Address1 = address1.Trim();
        Address2 = address2?.Trim();
        City = city.Trim();
        ZipCode = zipCode.Trim();
        CountryCode = countryCode.ToUpperInvariant();
        CountryId = countryId;
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        Phone = phone?.Trim();
        Company = company?.Trim();
        StateId = stateId;
        StateCode = stateCode?.ToUpperInvariant();

        return Result.Success;
    }

    public override string ToString()
    {
        var lines = new List<string?>
        {
            FullName,
            Company,
            Address1,
            Address2,
            $"{City}, {StateCode} {ZipCode}",
            CountryCode
        };
        return string.Join("\n", lines.Where(l => !string.IsNullOrWhiteSpace(l)));
    }
}