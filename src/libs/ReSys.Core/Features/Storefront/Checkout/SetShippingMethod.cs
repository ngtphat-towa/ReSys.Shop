using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Core.Features.Storefront.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Storefront.Checkout;

public static class SetShippingMethod
{
    public record Command(SetShippingMethodRequest Request) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.ShippingMethodId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            if (string.IsNullOrEmpty(userId)) return Error.Unauthorized();

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .Include(o => o.LineItems).ThenInclude(li => li.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
                .Include(o => o.ShipAddress)
                .Include(o => o.BillAddress)
                .FirstOrDefaultAsync(o => o.UserId == userId && (o.State == Order.OrderState.Address || o.State == Order.OrderState.Delivery), ct);

            if (order == null) return Error.NotFound("Order.NotFound", "No active checkout found.");

            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Request.ShippingMethodId && m.Status == ShippingMethod.ShippingStatus.Active, ct);

            if (method == null) return Error.NotFound("ShippingMethod.NotFound", "Invalid shipping method.");

            if (order.State == Order.OrderState.Address)
            {
                var transition = order.Next(); // Address -> Delivery
                if (transition.IsError) return transition.Errors;
            }

            var costCents = (long)(method.BaseCost * 100);
            var result = order.SetShippingMethod(method.Id, costCents);
            
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return order.Adapt<OrderDetailResponse>();
        }
    }
}