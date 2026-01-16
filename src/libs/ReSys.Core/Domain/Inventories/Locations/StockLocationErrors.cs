using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Locations;

public static class StockLocationErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "StockLocation.NotFound",
        description: $"Stock location with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "StockLocation.NameRequired",
        description: "Name is required.");

    public static Error HasStockItems => Error.Conflict(
        code: "StockLocation.HasStockItems",
        description: "Cannot delete location with existing stock items.");

    public static Error HasReservedStock => Error.Conflict(
        code: "StockLocation.HasReservedStock",
        description: "Cannot delete location with reserved stock.");
}
