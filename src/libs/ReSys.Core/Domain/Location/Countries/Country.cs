using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Location.Countries;

public sealed class Country : Aggregate
{
    public string Name { get; set; } = string.Empty;
    public string Iso { get; set; } = string.Empty;
    public string Iso3 { get; set; } = string.Empty;

    public Country() { }

    public static ErrorOr<Country> Create(string name, string iso, string iso3)
    {
        if (string.IsNullOrWhiteSpace(name)) return CountryErrors.NameRequired;
        if (name.Length > CountryConstraints.NameMaxLength) return CountryErrors.NameTooLong;
        
        if (string.IsNullOrWhiteSpace(iso)) return CountryErrors.IsoRequired;
        if (iso.Length != CountryConstraints.IsoMaxLength) return CountryErrors.InvalidIso;
        
        if (!string.IsNullOrWhiteSpace(iso3) && iso3.Length != CountryConstraints.Iso3MaxLength) 
            return CountryErrors.InvalidIso3;

        var country = new Country
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Iso = iso.Trim().ToUpperInvariant(),
            Iso3 = iso3.Trim().ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        country.RaiseDomainEvent(new CountryEvents.CountryCreated(country));
        return country;
    }

    public ErrorOr<Success> Update(string name, string iso, string iso3)
    {
        if (string.IsNullOrWhiteSpace(name)) return CountryErrors.NameRequired;
        if (name.Length > CountryConstraints.NameMaxLength) return CountryErrors.NameTooLong;
        
        if (string.IsNullOrWhiteSpace(iso)) return CountryErrors.IsoRequired;
        if (iso.Length != CountryConstraints.IsoMaxLength) return CountryErrors.InvalidIso;
        
        if (!string.IsNullOrWhiteSpace(iso3) && iso3.Length != CountryConstraints.Iso3MaxLength) 
            return CountryErrors.InvalidIso3;

        Name = name.Trim();
        Iso = iso.Trim().ToUpperInvariant();
        Iso3 = iso3.Trim().ToUpperInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new CountryEvents.CountryUpdated(this));
        return Result.Success;
    }
}
