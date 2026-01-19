using ErrorOr;

using ReSys.Core.Common.Data;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

/// <summary>
/// Defines the contract for fulfillment algorithms.
/// Each strategy provides a different logic for choosing warehouses (e.g., Greedy, Cost, Distance).
/// </summary>
public interface IFulfillmentStrategy
{
    /// <summary>
    /// Executes the specific logic to determine how stock should be allocated across the network for a specific store.
    /// </summary>
    Task<ErrorOr<FulfillmentPlan>> CreatePlanAsync(
        IApplicationDbContext context,
        Guid storeId,
        IDictionary<Guid, int> requestedItems,
        Guid? customerAddressId = null,
        CancellationToken ct = default);
}
