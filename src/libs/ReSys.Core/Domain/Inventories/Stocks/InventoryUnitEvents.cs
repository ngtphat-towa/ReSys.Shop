using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Stocks;

public static class InventoryUnitEvents
{
    public record InventoryUnitCreated(InventoryUnit Unit) : IDomainEvent;
    public record InventoryUnitDeleted(InventoryUnit Unit) : IDomainEvent;
    public record InventoryUnitRestored(InventoryUnit Unit) : IDomainEvent;
    public record InventoryUnitStateChanged(InventoryUnit Unit, InventoryUnitState OldState, InventoryUnitState NewState) : IDomainEvent;
}