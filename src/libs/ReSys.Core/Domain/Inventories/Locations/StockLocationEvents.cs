using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Locations;

public static class StockLocationEvents
{
    public record StockLocationCreated(StockLocation StockLocation) : IDomainEvent;
    public record StockLocationUpdated(StockLocation StockLocation) : IDomainEvent;
    public record StockLocationDeleted(StockLocation StockLocation) : IDomainEvent;
}
