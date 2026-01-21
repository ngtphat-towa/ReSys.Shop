using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Storefront.Ordering.Common;

namespace ReSys.Core.Features.Storefront.Cart;

public static class AddToCart
{
    public record Command(Guid VariantId, int Quantity, string? SessionId = null) : IRequest<ErrorOr<CartResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.VariantId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public class Handler(IApplicationDbContext dbContext, IUserContext userContext) : IRequestHandler<Command, ErrorOr<CartResponse>>
    {
        public async Task<ErrorOr<CartResponse>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            var sessionId = command.SessionId ?? userContext.AdhocCustomerId;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
                return Error.Validation("Cart.IdentityRequired", "Either an authenticated User or a Session ID is required.");

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .ThenInclude(li => li.InventoryUnits)
                .Include(o => o.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => 
                    o.State == Order.OrderState.Cart &&
                    ((userId != null && o.UserId == userId) || 
                     (userId == null && o.SessionId == sessionId)), ct);

            if (order == null)
            {
                var store = await dbContext.Set<Store>().FirstOrDefaultAsync(ct);
                if (store == null)
                {
                    return Error.Unexpected("Store.Missing", "System configuration error: No active store found.");
                }
                
                var createResult = Order.Create(store.Id, store.DefaultCurrency, userId, userContext.Email, sessionId);
                if (createResult.IsError) return createResult.Errors;
                
                order = createResult.Value;
                dbContext.Set<Order>().Add(order);
            }

            var variant = await dbContext.Set<Variant>()
                .Include(v => v.Product)
                .Include(v => v.OptionValues)
                .FirstOrDefaultAsync(v => v.Id == command.VariantId, ct);

            if (variant == null) return Error.NotFound("Catalog.VariantNotFound", "Product variant not found.");

            var result = order.AddVariant(variant, command.Quantity, DateTimeOffset.UtcNow);
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return order.Adapt<CartResponse>();
        }
    }
}
