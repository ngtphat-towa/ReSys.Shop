using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

namespace ReSys.Core.Features.Admin.Ordering.Fulfillment.GetFulfillmentProposal;

public static class GetFulfillmentProposal
{
    public record Query(
        Guid OrderId, 
        FulfillmentStrategyType Strategy = FulfillmentStrategyType.Greedy) : IRequest<ErrorOr<FulfillmentPlan>>;

    public class Handler(IApplicationDbContext context, IFulfillmentPlanner planner) : IRequestHandler<Query, ErrorOr<FulfillmentPlan>>
    {
        public async Task<ErrorOr<FulfillmentPlan>> Handle(Query query, CancellationToken ct)
        {
            // 1. Fetch Order items for the simulation
            var order = await context.Set<Order>()
                .AsNoTracking()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == query.OrderId, ct);

            if (order == null)
                return OrderErrors.NotFound(query.OrderId);

            // 2. Prepare the map for the planner
            var requestedItems = order.LineItems.ToDictionary(li => li.VariantId, li => li.Quantity);

            // 3. Request simulation (Read-Only)
            // Strategy filters warehouses based on StoreId relationship automatically now.
            return await planner.PlanFulfillmentAsync(
                order.StoreId, 
                requestedItems, 
                order.ShipAddressId, 
                ct, 
                query.Strategy);
        }
    }
}
