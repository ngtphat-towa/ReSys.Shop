using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Features.Admin.Inventories.Movements.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Inventories.Movements.GetStockMovementsPagedList;

public static class GetStockMovementsPagedList
{
    public record Request : QueryOptions
    {
        public Guid? StockItemId { get; set; }
        public Guid? VariantId { get; set; }
        public Guid? StockLocationId { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<StockMovementListItem>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<StockMovementListItem>>
    {
        public async Task<PagedList<StockMovementListItem>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<StockMovement>()
                .Include(x => x.StockItem)
                .AsNoTracking();

            // Filtering
            if (request.StockItemId.HasValue)
                dbQuery = dbQuery.Where(x => x.StockItemId == request.StockItemId.Value);

            if (request.VariantId.HasValue)
                dbQuery = dbQuery.Where(x => x.StockItem.VariantId == request.VariantId.Value);

            if (request.StockLocationId.HasValue)
                dbQuery = dbQuery.Where(x => x.StockItem.StockLocationId == request.StockLocationId.Value);

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderByDescending(x => x.CreatedAt);
            }

            return await sortedQuery.ToPagedListAsync<StockMovement, StockMovementListItem>(request, ct);
        }
    }
}
