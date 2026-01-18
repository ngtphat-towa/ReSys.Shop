using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public static class OptionTypeErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "OptionType.NotFound",
        description: $"Option type with ID '{id}' was not found.");

    public static Error DuplicateName => Error.Conflict(
        code: "OptionType.DuplicateName",
        description: "An option type with the same name already exists.");

    public static Error NameRequired => Error.Validation(
        code: "OptionType.NameRequired",
        description: "Option type name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "OptionType.NameTooLong",
        description: $"Option type name cannot exceed {OptionTypeConstraints.NameMaxLength} characters.");

    public static Error PresentationRequired => Error.Validation(
        code: "OptionType.PresentationRequired",
        description: "Option type presentation is required.");

    public static Error PresentationTooLong => Error.Validation(
        code: "OptionType.PresentationTooLong",
        description: $"Option type presentation cannot exceed {OptionTypeConstraints.PresentationMaxLength} characters.");

    public static Error CannotDeleteWithValues => Error.Conflict(
        code: "OptionType.CannotDeleteWithValues",
        description: "Cannot delete an option type that has associated values.");

    public static Error InvalidPosition => Error.Validation(
        code: "OptionType.InvalidPosition",
        description: $"Position must be greater than or equal to {OptionTypeConstraints.MinPosition}.");
}
