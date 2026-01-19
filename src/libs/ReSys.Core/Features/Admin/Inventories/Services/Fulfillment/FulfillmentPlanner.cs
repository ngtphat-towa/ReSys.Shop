using ErrorOr;

using ReSys.Core.Common.Data;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

public class FulfillmentPlanner(FulfillmentStrategyFactory factory, IApplicationDbContext context) : IFulfillmentPlanner
{
    public async Task<ErrorOr<FulfillmentPlan>> PlanFulfillmentAsync(
        Guid storeId,
        IDictionary<Guid, int> requestedItems,
        Guid? customerAddressId = null,
        CancellationToken ct = default,
        FulfillmentStrategyType strategyType = FulfillmentStrategyType.Greedy)
    {
        // 1. Get the strategy from the factory
        var strategy = factory.GetStrategy(strategyType);
        if (strategy.IsError)
            return strategy.Errors;

        // 2. Delegate the planning to the selected strategy
        return await strategy.Value.CreatePlanAsync(context, storeId, requestedItems, customerAddressId, ct);
    }
}
