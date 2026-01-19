namespace ReSys.Core.Domain.Inventories.Movements;

/// <summary>
/// Defines the specific business reason for an inventory change.
/// </summary>
public enum StockMovementType
{
    Adjustment, // Manual audit correction
    Receipt,    // From supplier / Purchase Order
    Sale,       // Finalized customer order
    Return,     // Returned by customer
    Loss,       // Damaged, stolen, or expired
    Transfer,   // Moving between internal locations
    TransferIn, // Received from another internal location
    TransferOut, // Sent to another internal location
    Correction  // Reversing an erroneous previous movement
}
