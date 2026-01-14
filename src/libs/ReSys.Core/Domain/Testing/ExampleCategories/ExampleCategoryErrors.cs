using ErrorOr;

namespace ReSys.Core.Domain.Testing.ExampleCategories;

public static class ExampleCategoryErrors
{
    public static Error NotFound => Error.NotFound(
        code: "ExampleCategory.NotFound",
        description: "Example category not found.");

    public static Error DuplicateName => Error.Conflict(
        code: "ExampleCategory.DuplicateName",
        description: "An example category with the same name already exists.");

    public static Error NameRequired => Error.Validation(
        code: "ExampleCategory.NameRequired",
        description: "Name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "ExampleCategory.NameTooLong",
        description: $"Name must not exceed {ExampleCategoryConstraints.NameMaxLength} characters.");

    public static Error DescriptionTooLong => Error.Validation(
        code: "ExampleCategory.DescriptionTooLong",
        description: $"Description must not exceed {ExampleCategoryConstraints.DescriptionMaxLength} characters.");
}
