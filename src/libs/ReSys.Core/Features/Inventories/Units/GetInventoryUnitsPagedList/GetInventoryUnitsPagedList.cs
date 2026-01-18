using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Inventories.Units.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Inventories.Units.GetInventoryUnitsPagedList;

public static class GetInventoryUnitsPagedList
{
    public record Request : QueryOptions
    {
        public Guid? StockItemId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ShipmentId { get; set; }
        public InventoryUnitState? State { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<InventoryUnitListItem>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<InventoryUnitListItem>>
    {
        public async Task<PagedList<InventoryUnitListItem>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<InventoryUnit>()
                .Include(x => x.StockItem)
                .AsNoTracking();

            // Filtering
            if (request.StockItemId.HasValue)
                dbQuery = dbQuery.Where(x => x.StockItemId == request.StockItemId.Value);

            if (request.OrderId.HasValue)
                dbQuery = dbQuery.Where(x => x.OrderId == request.OrderId.Value);

            if (request.ShipmentId.HasValue)
                dbQuery = dbQuery.Where(x => x.ShipmentId == request.ShipmentId.Value);

            if (request.State.HasValue)
                dbQuery = dbQuery.Where(x => x.State == request.State.Value);

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderByDescending(x => x.CreatedAt);
            }

            return await sortedQuery.ToPagedListAsync<InventoryUnit, InventoryUnitListItem>(request, ct);
        }
    }
}
