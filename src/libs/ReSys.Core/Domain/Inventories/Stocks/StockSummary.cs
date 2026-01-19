using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Inventories.Stocks;

/// <summary>
/// Represents a high-performance "Commercial Projection" of inventory for a variant.
/// Decouples front-facing availability checks from the heavy historical Ledger.
/// </summary>
/// <remarks>
/// <b>Golden Standard Logic:</b>
/// This entity is updated asynchronously via Domain Events. It provides O(1) lookups for 
/// storefront searches and product detail pages across 4,000+ products.
/// </remarks>
public sealed class StockSummary : Aggregate
{
    /// <summary>The Variant this summary describes.</summary>
    public Guid VariantId { get; set; }
    
    /// <summary>Total physical items across all fulfillable warehouses.</summary>
    public int TotalOnHand { get; set; }
    
    /// <summary>Total items promised to customers (Earmarked + Backordered).</summary>
    public int TotalReserved { get; set; }
    
    /// <summary>Mathematical difference (OnHand - Reserved). Can be negative if backorderable.</summary>
    public int TotalAvailable { get; set; }
    
    /// <summary>Commercial flag: True if the item can be added to a cart (Available > 0 OR Backorderable).</summary>
    public bool IsBuyable { get; set; }
    
    /// <summary>Cache of the variant's current backorder policy.</summary>
    public bool Backorderable { get; set; }

    public StockSummary() { }

    /// <summary>
    /// Factory for creating or updating the projection snapshot.
    /// </summary>
    public static StockSummary Create(
        Guid variantId, 
        int totalOnHand, 
        int totalReserved, 
        bool backorderable)
    {
        var available = backorderable 
            ? (totalOnHand - totalReserved) 
            : Math.Max(0, totalOnHand - totalReserved);

        return new StockSummary
        {
            Id = variantId, // Identity is shared with the Variant for direct lookup
            VariantId = variantId,
            TotalOnHand = totalOnHand,
            TotalReserved = totalReserved,
            TotalAvailable = totalOnHand - totalReserved, // Always true debt
            Backorderable = backorderable,
            IsBuyable = available > 0 || backorderable
        };
    }

    /// <summary>
    /// Updates the snapshot with new calculated values from the Ledger.
    /// </summary>
    public void Update(int totalOnHand, int totalReserved, bool backorderable)
    {
        TotalOnHand = totalOnHand;
        TotalReserved = totalReserved;
        TotalAvailable = totalOnHand - totalReserved;
        Backorderable = backorderable;
        
        var availableForSale = backorderable 
            ? TotalAvailable 
            : Math.Max(0, TotalAvailable);

        IsBuyable = availableForSale > 0 || backorderable;
    }
}
