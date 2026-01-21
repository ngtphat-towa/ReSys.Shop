using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Calculations;

namespace ReSys.Core.Features.Storefront.Cart;

public static class ApplyPromotionCode
{
    public record Command(string Code) : IRequest<ErrorOr<Success>>;

    public class Handler(
        IApplicationDbContext dbContext, 
        IUserContext userContext,
        IPromotionCalculator calculator) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            var sessionId = userContext.AdhocCustomerId;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId)) 
                return Error.Unauthorized();

            // 1. Find active cart
            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Classifications)
                .Include(o => o.OrderAdjustments)
                .FirstOrDefaultAsync(o => 
                    o.State == Order.OrderState.Cart &&
                    ((userId != null && o.UserId == userId) || 
                     (userId == null && o.SessionId == sessionId)), ct);

            if (order == null) return Error.NotFound("Order.NotFound", "No active cart found.");

            // 2. Find the promotion
            var promotion = await dbContext.Set<Promotion>()
                .Include(p => p.Rules)
                .Include(p => p.Action)
                .FirstOrDefaultAsync(p => p.Code == command.Code.Trim().ToUpperInvariant(), ct);

            // 3. Validation
            if (promotion == null) return Error.Validation("Promotion.InvalidCode", "The promotion code is invalid.");
            if (!promotion.IsActive) return Error.Validation("Promotion.Expired", "This promotion code has expired or reached its usage limit.");

            // 4. Domain Logic
            var result = order.ApplyPromotion(promotion, calculator);
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
