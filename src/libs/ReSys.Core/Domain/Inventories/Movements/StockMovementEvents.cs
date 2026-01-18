using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Movements;

public static class StockMovementEvents
{
    public record StockMovementCreated(StockMovement Movement) : IDomainEvent;
}