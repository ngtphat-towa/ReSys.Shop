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

    public static Error NameTooLong => Error.Validation(
        code: "PropertyType.NameTooLong",
        description: $"Name exceeds maximum length of {PropertyTypeConstraints.NameMaxLength} characters.");

    public static Error PresentationRequired => Error.Validation(
        code: "PropertyType.PresentationRequired",
        description: "Presentation is required.");

    public static Error PresentationTooLong => Error.Validation(
        code: "PropertyType.PresentationTooLong",
        description: $"Presentation exceeds maximum length of {PropertyTypeConstraints.PresentationMaxLength} characters.");

    public static Error DuplicateName => Error.Conflict(
        code: "PropertyType.DuplicateName",
        description: "A property type with the same name already exists.");

    public static Error InvalidKind => Error.Validation(
        code: "PropertyType.InvalidKind",
        description: $"Invalid property kind. Valid values are: {string.Join(", ", ReSys.Shared.Extensions.EnumExtensions.GetDescriptions<PropertyKind>())}.");

    public static Error InUse => Error.Conflict(
        code: "PropertyType.InUse",
        description: "Cannot delete property type that is currently in use by products.");

    public static Error InvalidPosition => Error.Validation(
        code: "PropertyType.InvalidPosition",
        description: $"Position must be greater than or equal to {PropertyTypeConstraints.MinPosition}.");
}
