using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering.LineItems;

/// <summary>
/// Represents a immutable snapshot of a product variant within a specific order.
/// Preserves pricing and naming truth at the exact moment of the transaction.
/// </summary>
public sealed class LineItem : Entity
{
    /// <summary>The parent order reference.</summary>
    public Guid OrderId { get; set; }

    /// <summary>The source variant reference (may become obsolete/deleted in catalog).</summary>
    public Guid VariantId { get; set; }

    /// <summary>Number of units ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Physical price per unit in cents (Price x 100) at time of order.</summary>
    public long PriceCents { get; set; }

    /// <summary>Currency code (e.g., 'USD') inherited from the order.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Snapshot of the product name for historical display.</summary>
    public string CapturedName { get; set; } = string.Empty;

    /// <summary>Snapshot of the variant SKU.</summary>
    public string? CapturedSku { get; set; }

    /// <summary>Flags if this item was added as part of a promotion (e.g., Free Gift).</summary>
    public bool IsPromotional { get; set; }

    // Relationships
    public Order Order { get; set; } = null!;
    public Variant Variant { get; set; } = null!;
    
    /// <summary>Granular link to physical inventory units.</summary>
    public ICollection<InventoryUnit> InventoryUnits { get; set; } = new List<InventoryUnit>();
    
    /// <summary>Discounts or fees applied specifically to this item.</summary>
    public ICollection<LineItemAdjustment> Adjustments { get; set; } = new List<LineItemAdjustment>();

    public LineItem() { }

    /// <summary>
    /// Factory for creating a sales line with a price snapshot.
    /// </summary>
    internal static ErrorOr<LineItem> Create(
        Guid orderId,
        Variant variant,
        int quantity,
        string currency,
        string capturedName,
        DateTimeOffset now,
        long? overridePriceCents = null)
    {
        // Guard: Logic invariants
        if (quantity < LineItemConstraints.MinQuantity) return LineItemErrors.InvalidQuantity;
        if (variant == null) return OrderErrors.VariantRequired;

        var finalPriceCents = overridePriceCents ?? (long)(variant.Price * 100);

        return new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variant.Id,
            Quantity = quantity,
            PriceCents = finalPriceCents, // Snapshot (Actual or Override)
            Currency = currency.ToUpperInvariant(),
            CapturedName = capturedName,
            CapturedSku = variant.Sku,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the quantity requested, triggering total recalculation in the root order.
    /// </summary>
    internal ErrorOr<Success> UpdateQuantity(int quantity, DateTimeOffset now)
    {
        if (quantity < LineItemConstraints.MinQuantity) return LineItemErrors.InvalidQuantity;
        
        Quantity = quantity;
        UpdatedAt = now;
        return Result.Success;
    }

    /// <summary>
    /// Calculates the item total in cents (Price * Qty + Adjustments).
    /// Used by the Order aggregate for totals recalculation.
    /// </summary>
    public long GetTotalCents()
    {
        var subtotal = PriceCents * Quantity;
        var adjustments = Adjustments.Where(a => a.Eligible).Sum(a => a.AmountCents);
        return subtotal + adjustments;
    }
}