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
    public Guid StockItemId { get; private set; }

    /// <summary>The change in quantity (positive for additions, negative for deductions).</summary>
    public int Quantity { get; private set; }

    /// <summary>The physical stock level before this movement occurred.</summary>
    public int BalanceBefore { get; private set; }

    /// <summary>The physical stock level after this movement was applied.</summary>
    public int BalanceAfter { get; private set; }

    /// <summary>The business categorization of the movement.</summary>
    public StockMovementType Type { get; private set; }

    /// <summary>Human-readable explanation for the adjustment.</summary>
    public string? Reason { get; private set; }

    /// <summary>External identifier linking the movement to an Order, Invoice, or PO.</summary>
    public string? Reference { get; private set; }

    /// <summary>The financial unit cost of the item at the exact moment of movement (for COGS calculation).</summary>
    public decimal UnitCost { get; private set; }

    // Navigation
    public StockItem StockItem { get; private set; } = null!;

    private StockMovement() { }

    /// <summary>
    /// Factory for creating a ledger entry. 
    /// Note: This is internal because only StockItem (Aggregate Root) should create movements.
    /// </summary>
    internal static StockMovement Create(
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
