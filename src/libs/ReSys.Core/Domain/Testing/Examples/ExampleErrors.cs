using ErrorOr;

namespace ReSys.Core.Domain.Testing.Examples;

public static class ExampleErrors
{
    public static Error DuplicateName => Error.Conflict(
        code: "Example.DuplicateName",
        description: "An example with the same name already exists.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Example.NotFound",
        description: $"The example with ID '{id}' was not found.");

    public static Error InvalidPrice => Error.Validation(
        code: "Example.InvalidPrice",
        description: $"Price must be greater than or equal to {ExampleConstraints.MinPrice}.");

    public static Error NameTooLong => Error.Validation(
        code: "Example.NameTooLong",
        description: $"Example name cannot exceed {ExampleConstraints.NameMaxLength} characters.");

    public static Error NameRequired => Error.Validation(
        code: "Example.NameRequired",
        description: "Example name is required.");

    public static Error DescriptionTooLong => Error.Validation(
        code: "Example.DescriptionTooLong",
        description: $"Example description cannot exceed {ExampleConstraints.DescriptionMaxLength} characters.");

    public static Error InvalidHexColor => Error.Validation(
        code: "Example.InvalidHexColor",
        description: "Invalid Hex Color format. It must be a valid hex code (e.g., #FFFFFF).");
}