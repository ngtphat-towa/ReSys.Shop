using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Domain.Inventories.Stocks;

public static class StockItemEvents
{
    public record StockItemCreated(StockItem StockItem) : IDomainEvent;
    public record StockItemDeleted(StockItem StockItem) : IDomainEvent;
    public record StockItemRestored(StockItem StockItem) : IDomainEvent;

    // Ledger Events
    public record StockAdjusted(StockItem StockItem, int Adjustment, StockMovementType Type, string? Reference) : IDomainEvent;

    // State Events
    public record StockReserved(StockItem StockItem, int Quantity, string? Reference) : IDomainEvent;
    public record StockReleased(StockItem StockItem, int Quantity, string? Reference) : IDomainEvent;
    public record StockFilled(StockItem StockItem, int Quantity, string Reference) : IDomainEvent;

    // Configuration Events
    public record BackorderPolicyChanged(StockItem StockItem, bool Backorderable, int Limit) : IDomainEvent;
}
