using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Stocks;

public static class StockItemEvents
{
    public record StockItemCreated(StockItem StockItem) : IDomainEvent;
    public record StockRestocked(StockItem StockItem, int Quantity) : IDomainEvent;
    public record StockReserved(StockItem StockItem, int Quantity) : IDomainEvent;
    public record StockReleased(StockItem StockItem, int Quantity) : IDomainEvent;
    public record StockFilled(StockItem StockItem, int Quantity) : IDomainEvent;
}