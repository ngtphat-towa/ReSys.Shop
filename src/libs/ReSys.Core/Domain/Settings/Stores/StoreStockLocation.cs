using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Domain.Settings.Stores;

public sealed class StoreStockLocation : Entity
{
    public Guid StoreId { get; set; }
    public Guid StockLocationId { get; set; }

    // Relationship Metadata
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;

    // Navigation Properties
    public Store Store { get; set; } = null!;
    public StockLocation StockLocation { get; set; } = null!;

    private StoreStockLocation() { }

    internal static StoreStockLocation Create(Guid storeId, Guid stockLocationId, bool isActive = true, int priority = 0)
    {
        return new StoreStockLocation
        {
            StoreId = storeId,
            StockLocationId = stockLocationId,
            IsActive = isActive,
            Priority = priority
        };
    }

    public void SetPriority(int priority) => Priority = priority;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
