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
        description: "Name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "OptionType.NameTooLong",
        description: $"Name exceeds maximum length of {OptionTypeConstraints.NameMaxLength} characters.");

    public static Error PresentationRequired => Error.Validation(
        code: "OptionType.PresentationRequired",
        description: "Presentation is required.");

    public static Error PresentationTooLong => Error.Validation(
        code: "OptionType.PresentationTooLong",
        description: $"Presentation exceeds maximum length of {OptionTypeConstraints.PresentationMaxLength} characters.");

    public static Error HasProductInUse => Error.Conflict(
        code: "OptionType.HasProductInUse",
        description: "Cannot delete option type with existing product in use.");

    public static Error CannotDeleteWithValues => Error.Conflict(
        code: "OptionType.CannotDeleteWithValues",
        description: "Cannot delete option type that has associated values. Remove values first.");
}
