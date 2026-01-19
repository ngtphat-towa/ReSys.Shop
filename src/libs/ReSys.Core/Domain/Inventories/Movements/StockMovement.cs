using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Domain.Inventories.Movements;

/// <summary>
/// Represents an immutable ledger entry for every physical inventory change.
/// Provides the audit trail for financial valuation and stock reconciliation.
/// </summary>
public sealed class StockMovement : Entity, IAuditable
{
    /// <summary>The unique identifier of the stock item this movement belongs to.</summary>
    public Guid StockItemId { get; set; }

    /// <summary>The change in quantity (positive for additions, negative for deductions).</summary>
    public int Quantity { get; set; }

    /// <summary>The physical stock level before this movement occurred.</summary>
    public int BalanceBefore { get; set; }

    /// <summary>The physical stock level after this movement was applied.</summary>
    public int BalanceAfter { get; set; }

    /// <summary>The business categorization of the movement.</summary>
    public StockMovementType Type { get; set; }

    /// <summary>Human-readable explanation for the adjustment.</summary>
    public string? Reason { get; set; }

    /// <summary>External identifier linking the movement to an Order, Invoice, or PO.</summary>
    public string? Reference { get; set; }

    /// <summary>The financial unit cost of the item at the exact moment of movement (for COGS calculation).</summary>
    public decimal UnitCost { get; set; }

    // Navigation
    public StockItem StockItem { get; set; } = null!;

    public StockMovement() { }

    /// <summary>
    /// Factory for creating a ledger entry. 
    /// </summary>
    public static StockMovement Create(
        Guid stockItemId,
        int quantity,
        int balanceBefore,
        StockMovementType type,
        decimal unitCost = 0,
        string? reason = null,
        string? reference = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            StockItemId = stockItemId,
            Quantity = quantity,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceBefore + quantity,
            Type = type,
            UnitCost = unitCost,
            Reason = reason?.Trim(),
            Reference = reference?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
