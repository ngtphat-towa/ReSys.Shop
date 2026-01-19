using ErrorOr;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

/// <summary>
/// The "Predictive Brain" of the logistics system.
/// Acts as the bridge between Commercial Intent (Orders) and Physical Reality (Inventory).
/// </summary>
/// <remarks>
/// <b>CORE PRINCIPLES:</b>
/// <list type="bullet">
/// <item><b>Simulation (Read-Only):</b> The planner only proposes a map; it does not reserve rows in the DB.</item>
/// <item><b>Efficiency (Single-Box Goal):</b> Attempts to satisfy the order from the minimum number of warehouses.</item>
/// <item><b>Network Awareness:</b> Respects 'IsFulfillable' flags to avoid 'Damaged' or 'Transit' zones.</item>
/// <item><b>Store Context:</b> Only considers warehouses that are specifically linked and active for the requested store.</item>
/// </list>
/// </remarks>
public interface IFulfillmentPlanner
{
    /// <summary>
    /// Calculates the optimal routing of stock across the warehouse network linked to a specific store.
    /// </summary>
    /// <param name="storeId">The unique identifier of the store (branch) placing the order.</param>
    /// <param name="requestedItems">A map of Variant IDs and the requested physical quantity.</param>
    /// <param name="customerAddressId">Optional: Used by advanced implementations to calculate geographic proximity.</param>
    /// <returns>A proposed FulfillmentPlan containing grouped shipments and backorder status.</returns>
    Task<ErrorOr<FulfillmentPlan>> PlanFulfillmentAsync(
        Guid storeId,
        IDictionary<Guid, int> requestedItems, 
        Guid? customerAddressId = null, 
        CancellationToken ct = default,
        FulfillmentStrategyType strategyType = FulfillmentStrategyType.Greedy);
}
