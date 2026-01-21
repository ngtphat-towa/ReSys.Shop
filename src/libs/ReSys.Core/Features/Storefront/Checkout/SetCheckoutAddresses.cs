using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Storefront.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Storefront.Checkout;

public static class SetCheckoutAddresses
{
    public record Command(SetCheckoutAddressesRequest Request) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.ShippingAddressId).NotEmpty();
            RuleFor(x => x.Request.BillingAddressId).NotEmpty();
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
                .FirstOrDefaultAsync(o => o.UserId == userId && (o.State == Order.OrderState.Cart || o.State == Order.OrderState.Address), ct);

            if (order == null) return Error.NotFound("Order.NotFound", "No active cart found.");

            var addresses = await dbContext.Set<UserAddress>()
                .Where(a => a.UserId == userId && (a.Id == command.Request.ShippingAddressId || a.Id == command.Request.BillingAddressId))
                .ToListAsync(ct);

            var shipping = addresses.FirstOrDefault(a => a.Id == command.Request.ShippingAddressId);
            var billing = addresses.FirstOrDefault(a => a.Id == command.Request.BillingAddressId);

            if (shipping == null || billing == null) return Error.NotFound("Address.NotFound", "One or more addresses not found.");

            if (order.State == Order.OrderState.Cart)
            {
                var transition = order.Next(); // Cart -> Address
                if (transition.IsError) return transition.Errors;
            }

            var result = order.SetAddresses(shipping, billing);
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return order.Adapt<OrderDetailResponse>();
        }
    }
}