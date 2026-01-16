using ErrorOr;

namespace ReSys.Core.Domain.Catalog.PropertyTypes;

public static class PropertyTypeErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "PropertyType.NotFound",
        description: $"Property type with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "PropertyType.NameRequired",
        description: "Name is required.");
}
