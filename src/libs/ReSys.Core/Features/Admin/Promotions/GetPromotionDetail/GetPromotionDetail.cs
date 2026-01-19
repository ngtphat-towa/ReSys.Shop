using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Features.Admin.Promotions.Common;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Admin.Promotions.GetPromotionDetail;

public static class GetPromotionDetail
{
    public record Query(Guid Id) : IRequest<ErrorOr<PromotionResponse>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<PromotionResponse>>
    {
        public async Task<ErrorOr<PromotionResponse>> Handle(Query request, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (promotion is null) return PromotionErrors.NotFound(request.Id);

            return promotion.Adapt<PromotionResponse>();
        }
    }
}
