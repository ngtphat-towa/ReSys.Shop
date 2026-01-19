using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Usages;
using ErrorOr;

namespace ReSys.Core.Features.Promotions.Admin.GetPromotionStats;

public static class GetPromotionStats
{
    public record Response
    {
        public Guid PromotionId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int TotalUsages { get; init; }
        public decimal TotalDiscountAmount { get; init; }
        public decimal AverageDiscountPerOrder { get; init; }
        public DateTimeOffset? LastUsedAt { get; init; }
    }

    public record Query(Guid Id) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (promotion == null) return PromotionErrors.NotFound(request.Id);

            var usages = await context.Set<PromotionUsage>()
                .Where(u => u.PromotionId == request.Id)
                .ToListAsync(ct);

            var totalUsages = usages.Count;
            var totalDiscountCents = usages.Sum(u => u.DiscountAmountCents);
            var totalDiscount = totalDiscountCents / 100m;

            return new Response
            {
                PromotionId = promotion.Id,
                Name = promotion.Name,
                TotalUsages = totalUsages,
                TotalDiscountAmount = totalDiscount,
                AverageDiscountPerOrder = totalUsages > 0 ? Math.Round(totalDiscount / totalUsages, 2) : 0,
                LastUsedAt = usages.OrderByDescending(u => u.CreatedAt).FirstOrDefault()?.CreatedAt
            };
        }
    }
}