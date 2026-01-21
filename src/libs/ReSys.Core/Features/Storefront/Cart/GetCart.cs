using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Storefront.Ordering.Common;

namespace ReSys.Core.Features.Storefront.Cart;

public static class GetCart
{
    public record Query(string? SessionId = null) : IRequest<ErrorOr<CartResponse>>;

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Query, ErrorOr<CartResponse>>
    {
        public async Task<ErrorOr<CartResponse>> Handle(Query query, CancellationToken ct)
        {
            var userId = userContext.UserId;
            var sessionId = query.SessionId ?? userContext.AdhocCustomerId;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
                return new CartResponse(); // Return empty cart response

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => 
                    o.State == Order.OrderState.Cart &&
                    ((userId != null && o.UserId == userId) || 
                     (userId == null && o.SessionId == sessionId)), ct);

            if (order == null)
            {
                return new CartResponse();
            }

            return order.Adapt<CartResponse>();
        }
    }
}
