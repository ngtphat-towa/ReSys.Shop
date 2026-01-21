using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;

namespace ReSys.Core.Features.Storefront.Cart;

public static class RemovePromotion
{
    public record Command : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            var sessionId = userContext.AdhocCustomerId;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId)) return Error.Unauthorized();

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Adjustments)
                .Include(o => o.OrderAdjustments)
                .FirstOrDefaultAsync(o => 
                    o.State == Order.OrderState.Cart &&
                    ((userId != null && o.UserId == userId) || 
                     (userId == null && o.SessionId == sessionId)), ct);

            if (order == null) return Error.NotFound("Order.NotFound", "No active cart found.");

            var result = order.RemovePromotion();
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
