using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Storefront.Checkout;

public static class GetShippingMethods
{
    public record Query : IRequest<ErrorOr<List<Result>>>;

    public record Result(Guid Id, string Name, string Description, long PriceCents, string EstimatedDelivery);

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Query, ErrorOr<List<Result>>>
    {
        public async Task<ErrorOr<List<Result>>> Handle(Query query, CancellationToken ct)
        {
            var userId = userContext.UserId;
            if (string.IsNullOrEmpty(userId)) return Error.Unauthorized();

            // 1. Get Order Context
            var order = await dbContext.Set<Order>()
                .Include(o => o.ShipAddress)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId && (o.State == Order.OrderState.Address || o.State == Order.OrderState.Delivery), ct);

            if (order == null || order.ShipAddress == null) return Error.Validation("Order.AddressRequired", "Shipping address not set.");

            // 2. Fetch Eligible Methods
            var methods = await dbContext.Set<ShippingMethod>()
                .Where(m => m.Status == ShippingMethod.ShippingStatus.Active)
                .OrderBy(m => m.Position)
                .AsNoTracking()
                .ToListAsync(ct);

            // 3. Map
            return methods.Select(m => new Result(
                m.Id,
                m.Name,
                m.Description ?? "",
                (long)(m.BaseCost * 100),
                m.Type.ToString() // Placeholder for delivery estimate
            )).ToList();
        }
    }
}
