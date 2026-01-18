using ErrorOr;

namespace ReSys.Core.Features.Inventories.Services.Fulfillment;

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
/// </list>
/// </remarks>
public interface IFulfillmentPlanner
{
    /// <summary>
    /// Calculates the optimal routing of stock across the entire warehouse network.
    /// </summary>
    /// <param name="requestedItems">A map of Variant IDs and the requested physical quantity.</param>
    /// <param name="customerAddressId">Optional: Used by advanced implementations to calculate geographic proximity.</param>
    /// <returns>A proposed FulfillmentPlan containing grouped shipments and backorder status.</returns>
    Task<ErrorOr<FulfillmentPlan>> PlanFulfillmentAsync(
        IDictionary<Guid, int> requestedItems, 
        Guid? customerAddressId = null, 
        CancellationToken ct = default);
}