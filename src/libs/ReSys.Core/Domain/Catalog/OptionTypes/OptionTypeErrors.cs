using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public static class OptionTypeErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "OptionType.NotFound",
        description: $"Option type with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "OptionType.NameRequired",
        description: "Name is required.");

    public static Error HasProductInUse => Error.Conflict(
        code: "OptionType.HasProductInUse",
        description: "Cannot delete option type with existing product in use.");
}
