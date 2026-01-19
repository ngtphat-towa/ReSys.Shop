using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Orders.GetOrderDetail;

public static class GetOrderDetail
{
    public record Query(Guid Id) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Query query, CancellationToken ct)
        {
            // 1. Fetch the Complete Truth (Heavy Graph)
            // We use AsNoTracking for read performance, but include all intersecting ledgers.
            var order = await context.Set<Order>()
                .AsNoTracking()
                .Include(x => x.LineItems)
                    .ThenInclude(li => li.Adjustments)
                .Include(x => x.LineItems)
                    .ThenInclude(li => li.InventoryUnits)
                .Include(x => x.OrderAdjustments)
                .Include(x => x.Payments)
                .Include(x => x.Shipments)
                    .ThenInclude(s => s.InventoryUnits)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

            if (order == null)
                return OrderErrors.NotFound(query.Id);

            // 2. Map to the High-Precision DTO
            return order.Adapt<OrderDetailResponse>();
        }
    }
}
