using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Admin.Ordering.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Ordering.Orders.GetOrdersPagedList;

public static class GetOrdersPagedList
{
    public record Request : QueryOptions
    {
        public Guid? StoreId { get; set; }
        public Guid? WarehouseId { get; set; }
        public Order.OrderState? State { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<OrderSummaryResponse>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<OrderSummaryResponse>>
    {
        public async Task<PagedList<OrderSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<Order>()
                .AsNoTracking();

            // 1. Core Filters
            if (request.StoreId.HasValue)
                dbQuery = dbQuery.Where(x => x.StoreId == request.StoreId.Value);

            if (request.State.HasValue)
                dbQuery = dbQuery.Where(x => x.State == request.State.Value);

            if (request.WarehouseId.HasValue)
                dbQuery = dbQuery.Where(x => x.Shipments.Any(s => s.StockLocationId == request.WarehouseId.Value));

            if (request.FromDate.HasValue)
                dbQuery = dbQuery.Where(x => x.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                dbQuery = dbQuery.Where(x => x.CreatedAt <= request.ToDate.Value);

            // 2. Search (Number, Email, or Item SKU)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(x => 
                    x.Number.ToLower().Contains(searchTerm) || 
                    (x.Email != null && x.Email.ToLower().Contains(searchTerm)) ||
                    x.LineItems.Any(li => li.CapturedSku != null && li.CapturedSku.ToLower().Contains(searchTerm)));
            }

            // 3. Paging & Sorting
            return await dbQuery
                .ApplyFilter(request)
                .ApplySort(request)
                .ToPagedListAsync<Order, OrderSummaryResponse>(request, ct);
        }
    }
}
