using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Locations;

public static class StockLocationEvents
{
    public record StockLocationCreated(StockLocation Location) : IDomainEvent;
    public record StockLocationUpdated(StockLocation Location) : IDomainEvent;
    public record StockLocationDeleted(StockLocation Location) : IDomainEvent;
    public record StockLocationRestored(StockLocation Location) : IDomainEvent;
    public record StockLocationActivated(StockLocation Location) : IDomainEvent;
    public record StockLocationDeactivated(StockLocation Location) : IDomainEvent;
}
