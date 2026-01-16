using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public static class OptionValueErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "OptionValue.NotFound",
        description: $"Option value with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "OptionValue.NameRequired",
        description: "Name is required.");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        code: "OptionValue.NameAlreadyExists",
        description: $"Option value with name '{name}' already exists for this option type.");
}
