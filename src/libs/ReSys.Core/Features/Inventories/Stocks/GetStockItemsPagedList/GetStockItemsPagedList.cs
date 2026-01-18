using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Inventories.Stocks.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Inventories.Stocks.GetStockItemsPagedList;

public static class GetStockItemsPagedList
{
    public record Request : QueryOptions
    {
        public Guid? VariantId { get; set; }
        public Guid? StockLocationId { get; set; }
    }

    public record Response : StockItemListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<StockItem>()
                .Include(x => x.Variant).ThenInclude(v => v.Product)
                .Include(x => x.StockLocation)
                .Where(x => !x.IsDeleted)
                .AsNoTracking();

            if (request.VariantId.HasValue)
                dbQuery = dbQuery.Where(x => x.VariantId == request.VariantId.Value);

            if (request.StockLocationId.HasValue)
                dbQuery = dbQuery.Where(x => x.StockLocationId == request.StockLocationId.Value);

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Sku);
            }

            return await sortedQuery.ToPagedListAsync<StockItem, Response>(request, ct);
        }
    }
}
