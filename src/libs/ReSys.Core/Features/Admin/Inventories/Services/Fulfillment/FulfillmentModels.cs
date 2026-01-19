using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

/// <summary>
/// Represents a proposed shipment from a specific physical warehouse.
/// Part of a simulated fulfillment plan that groups items to minimize shipping costs.
/// </summary>
public record FulfillmentShipment
{
    /// <summary>The physical source of the items.</summary>
    public Guid StockLocationId { get; init; }
    
    /// <summary>The human-readable name of the warehouse for UI display.</summary>
    public string StockLocationName { get; init; } = null!;
    
    /// <summary>List of items allocated to this specific shipment package.</summary>
    public List<FulfillmentItem> Items { get; init; } = [];
}

/// <summary>
/// Represents a specific quantity of a product variant allocated from a warehouse.
/// Distinguishes between physical 'OnHand' stock and logical 'Backordered' promises.
/// </summary>
public record FulfillmentItem
{
    public Guid VariantId { get; init; }
    public string Sku { get; init; } = null!;
    public int Quantity { get; init; }
    
    /// <summary>
    /// If true, this item is not physically present at the location and represents 
    /// a fulfillment debt to be resolved when new stock arrives.
    /// </summary>
    public bool IsBackordered { get; init; }
}

/// <summary>
/// The final output of the Fulfillment Engine. 
/// This is a "Simulation" result and does not modify the database until executed by a handler.
/// </summary>
public record FulfillmentPlan
{
    /// <summary>The collection of proposed shipments required to satisfy the order.</summary>
    public List<FulfillmentShipment> Shipments { get; init; } = [];
    
    /// <summary>True if the entire order can be satisfied from physical On-Hand stock.</summary>
    public bool IsFullFulfillment => Shipments.SelectMany(s => s.Items).All(i => !i.IsBackordered);
}
