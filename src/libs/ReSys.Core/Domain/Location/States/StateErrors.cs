using ErrorOr;

namespace ReSys.Core.Domain.Location.States;

public static class StateErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "State.NotFound",
        description: $"State with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "State.NameRequired",
        description: "State name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "State.NameTooLong",
        description: $"State name cannot exceed {StateConstraints.NameMaxLength} characters.");

    public static Error CountryIdRequired => Error.Validation(
        code: "State.CountryIdRequired",
        description: "Country ID is required.");
    
    public static Error AbbrTooLong => Error.Validation(
        code: "State.AbbrTooLong",
        description: $"Abbreviation cannot exceed {StateConstraints.AbbrMaxLength} characters.");
}
