using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Promotions.Usages;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

using ErrorOr;

namespace ReSys.Core.Features.Promotions.Admin.GetPromotionUsageHistory;

public static class GetPromotionUsageHistory
{
    public record Response
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public string? UserId { get; init; }
        public decimal DiscountAmount { get; init; }
        public string? AppliedCode { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record Query(Guid Id, QueryOptions Options) : IRequest<ErrorOr<PagedList<Response>>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<PagedList<Response>>>
    {
        public async Task<ErrorOr<PagedList<Response>>> Handle(Query query, CancellationToken ct)
        {
            var dbQuery = context.Set<PromotionUsage>()
                .AsNoTracking()
                .Where(u => u.PromotionId == query.Id)
                .OrderByDescending(u => u.CreatedAt);

            return await dbQuery
                .Select(u => new Response
                {
                    Id = u.Id,
                    OrderId = u.OrderId,
                    UserId = u.UserId,
                    DiscountAmount = u.DiscountAmountCents / 100m,
                    AppliedCode = u.AppliedCode,
                    CreatedAt = u.CreatedAt
                })
                .ToPagedListAsync<Response, Response>(query.Options, ct);
        }
    }
}