using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Calculations;
using ErrorOr;

namespace ReSys.Core.Features.Admin.Promotions.PreviewPromotion;

public static class PreviewPromotion
{
    public record PreviewItem(Guid VariantId, int Quantity);
    
    public record Request(List<PreviewItem> Items, Guid? StoreId = null);
    
    public record Response(
        bool IsEligible,
        decimal TotalDiscount,
        List<PromotionAdjustment> Adjustments,
        string? ErrorMessage = null);

    public record Query(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(
        IApplicationDbContext context,
        IPromotionCalculator calculator) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .Include(p => p.Rules)
                .Include(p => p.Action)
                .FirstOrDefaultAsync(p => p.Id == query.Id, ct);

            if (promotion is null) return PromotionErrors.NotFound(query.Id);

            var variantIds = query.Request.Items.Select(i => i.VariantId).ToList();
            var variants = await context.Set<Variant>()
                .Include(v => v.Product)
                    .ThenInclude(p => p.Classifications)
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(ct);

            var storeId = query.Request.StoreId ?? Guid.Empty;
            var orderResult = Order.Create(storeId, "USD");
            if (orderResult.IsError) return orderResult.Errors;
            var order = orderResult.Value;

            foreach (var itemRequest in query.Request.Items)
            {
                var variant = variants.FirstOrDefault(v => v.Id == itemRequest.VariantId);
                if (variant == null) continue;

                order.AddVariant(variant, itemRequest.Quantity, DateTimeOffset.UtcNow);
            }

            var result = calculator.Calculate(promotion, order);

            if (result.IsError)
            {
                return new Response(
                    IsEligible: false, 
                    TotalDiscount: 0, 
                    Adjustments: [], 
                    ErrorMessage: result.FirstError.Description);
            }

            return new Response(
                IsEligible: true,
                TotalDiscount: result.Value.Adjustments.Sum(a => a.AmountCents) / -100m,
                Adjustments: result.Value.Adjustments.ToList()
            );
        }
    }
}
