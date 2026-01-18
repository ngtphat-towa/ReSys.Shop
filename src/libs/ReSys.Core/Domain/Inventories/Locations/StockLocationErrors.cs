using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Locations;

public static class StockLocationErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "StockLocation.NotFound",
        description: $"Stock location with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "StockLocation.NameRequired",
        description: "Stock location name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "StockLocation.NameTooLong",
        description: $"Stock location name cannot exceed {StockLocationConstraints.NameMaxLength} characters.");

    public static Error CodeRequired => Error.Validation(
        code: "StockLocation.CodeRequired",
        description: "Stock location code is required.");

    public static Error InvalidCodeFormat => Error.Validation(
        code: "StockLocation.InvalidCodeFormat",
        description: "Code must be uppercase alphanumeric with hyphens or underscores.");

    public static Error DuplicateCode(string code) => Error.Conflict(
        code: "StockLocation.DuplicateCode",
        description: $"Stock location with code '{code}' already exists.");

    public static Error CannotDeactivateDefault => Error.Conflict(
        code: "StockLocation.CannotDeactivateDefault",
        description: "Default stock location cannot be deactivated.");

    public static Error CannotDeleteDefault => Error.Conflict(
        code: "StockLocation.CannotDeleteDefault",
        description: "Default stock location cannot be deleted.");
}
