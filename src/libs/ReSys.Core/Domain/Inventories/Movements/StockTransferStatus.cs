namespace ReSys.Core.Domain.Inventories.Movements;

/// <summary>
/// Defines the lifecycle stages of a stock transfer between locations.
/// </summary>
public enum StockTransferStatus
{
    Draft,      // Items being added, stock not yet moved
    InTransit,  // Shipped from source, not yet at destination
    Completed,  // Received at destination
    Canceled    // Aborted before completion
}
