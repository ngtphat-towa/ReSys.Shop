namespace ReSys.Core.Domain.Inventories.Locations;

public enum StockLocationType
{
    Warehouse,      // Primary storage (Fulfillable)
    RetailStore,    // Physical storefront (Fulfillable)
    ReturnCenter,   // Where returns are processed (Non-Fulfillable)
    Transit,        // Virtual location for items moving (Non-Fulfillable)
    Damaged         // Non-sellable items (Non-Fulfillable)
}
