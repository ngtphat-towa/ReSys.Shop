using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

public static class OptionValueErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "OptionValue.NotFound",
        description: $"Option value with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "OptionValue.NameRequired",
        description: "Name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "OptionValue.NameTooLong",
        description: $"Name exceeds maximum length of {OptionValueConstraints.NameMaxLength} characters.");

    public static Error PresentationRequired => Error.Validation(
        code: "OptionValue.PresentationRequired",
        description: "Presentation is required.");

    public static Error PresentationTooLong => Error.Validation(
        code: "OptionValue.PresentationTooLong",
        description: $"Presentation exceeds maximum length of {OptionValueConstraints.PresentationMaxLength} characters.");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        code: "OptionValue.NameAlreadyExists",
        description: $"Option value with name '{name}' already exists for this option type.");

    public static Error InvalidPosition => Error.Validation(
        code: "OptionValue.InvalidPosition",
        description: $"Position must be greater than or equal to {OptionValueConstraints.MinPosition}.");
}