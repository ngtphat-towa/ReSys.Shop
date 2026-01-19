using ErrorOr;

namespace ReSys.Core.Domain.Settings.Stores;

public static class StoreErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Store.NotFound",
        description: $"Store with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Store.NameRequired",
        description: "Store name is required.");

    public static Error CodeRequired => Error.Validation(
        code: "Store.CodeRequired",
        description: "Store code is required.");

    public static Error CurrencyRequired => Error.Validation(
        code: "Store.CurrencyRequired",
        description: "Default currency is required.");

    public static Error InvalidCodeFormat => Error.Validation(
        code: "Store.InvalidCodeFormat",
        description: "Store code must be uppercase alphanumeric with underscores.");

    public static Error DuplicateCode => Error.Conflict(
        code: "Store.DuplicateCode",
        description: "A store with this code already exists.");

    public static Error CannotRemoveDefaultLocation => Error.Conflict(
        code: "Store.CannotRemoveDefaultLocation",
        description: "Cannot remove the default stock location. Assign a new default first.");
}
