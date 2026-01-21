using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Storefront.Ordering.Common;

namespace ReSys.Core.Features.Storefront.Cart;

public static class MergeCart
{
    public record Command(string SessionId) : IRequest<ErrorOr<CartResponse>>;

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Command, ErrorOr<CartResponse>>
    {
        public async Task<ErrorOr<CartResponse>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            if (string.IsNullOrEmpty(userId)) return Error.Unauthorized();

            if (string.IsNullOrEmpty(command.SessionId)) 
            {
                var currentCart = await dbContext.Set<Order>()
                    .Include(o => o.LineItems)
                    .ThenInclude(li => li.Variant)
                    .ThenInclude(v => v.Product)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.State == Order.OrderState.Cart, ct);
                return currentCart?.Adapt<CartResponse>() ?? new CartResponse();
            }

            // 1. Find Guest Cart (Source)
            var guestCart = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.OptionValues)
                .FirstOrDefaultAsync(o => o.SessionId == command.SessionId && o.State == Order.OrderState.Cart, ct);

            // 2. Find User Cart (Target)
            var userCart = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.State == Order.OrderState.Cart, ct);

            if (guestCart == null || !guestCart.LineItems.Any()) 
                return userCart?.Adapt<CartResponse>() ?? new CartResponse();

            if (userCart == null)
            {
                guestCart.UserId = userId;
                guestCart.SessionId = null; 
                await dbContext.SaveChangesAsync(ct);
                return guestCart.Adapt<CartResponse>();
            }

            foreach (var guestItem in guestCart.LineItems)
            {
                if (guestItem.Variant != null)
                {
                    userCart.AddVariant(guestItem.Variant, guestItem.Quantity, DateTimeOffset.UtcNow);
                }
            }

            dbContext.Set<Order>().Remove(guestCart);
            await dbContext.SaveChangesAsync(ct);
            return userCart.Adapt<CartResponse>();
        }
    }
}
