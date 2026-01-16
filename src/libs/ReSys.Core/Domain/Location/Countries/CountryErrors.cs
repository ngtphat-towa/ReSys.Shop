using ErrorOr;

namespace ReSys.Core.Domain.Location.Countries;

public static class CountryErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Country.NotFound",
        description: $"Country with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Country.NameRequired",
        description: "Country name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "Country.NameTooLong",
        description: $"Country name cannot exceed {CountryConstraints.NameMaxLength} characters.");

    public static Error IsoRequired => Error.Validation(
        code: "Country.IsoRequired",
        description: "ISO Alpha-2 code is required.");

    public static Error InvalidIso => Error.Validation(
        code: "Country.InvalidIso",
        description: $"ISO Alpha-2 code must be exactly {CountryConstraints.IsoMaxLength} characters.");

    public static Error InvalidIso3 => Error.Validation(
        code: "Country.InvalidIso3",
        description: $"ISO Alpha-3 code must be exactly {CountryConstraints.Iso3MaxLength} characters.");
}
