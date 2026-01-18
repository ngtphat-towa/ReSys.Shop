using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Movements;

public static class StockTransferEvents
{
    public record StockTransferCreated(StockTransfer Transfer) : IDomainEvent;
    public record StockTransferShipped(StockTransfer Transfer) : IDomainEvent;
    public record StockTransferReceived(StockTransfer Transfer) : IDomainEvent;
    public record StockTransferCanceled(StockTransfer Transfer) : IDomainEvent;
    public record StockTransferItemAdded(StockTransfer Transfer, StockTransferItem Item) : IDomainEvent;
}
