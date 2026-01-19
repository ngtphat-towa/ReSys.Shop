using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Features.Admin.Inventories.Transfers.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.GetStockTransfersPagedList;

public static class GetStockTransfersPagedList
{
    public record Request : QueryOptions
    {
        public Guid? SourceLocationId { get; set; }
        public Guid? DestinationLocationId { get; set; }
        public StockTransferStatus? Status { get; set; }
    }

    public record Response : StockTransferListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            // Projecting with Names via a Join
            var dbQuery = from t in context.Set<StockTransfer>()
                          join src in context.Set<Domain.Inventories.Locations.StockLocation>() on t.SourceLocationId equals src.Id
                          join dest in context.Set<Domain.Inventories.Locations.StockLocation>() on t.DestinationLocationId equals dest.Id
                          select new Response
                          {
                              Id = t.Id,
                              ReferenceNumber = t.ReferenceNumber,
                              SourceLocationId = t.SourceLocationId,
                              SourceLocationName = src.Name,
                              DestinationLocationId = t.DestinationLocationId,
                              DestinationLocationName = dest.Name,
                              Status = t.Status.ToString(),
                              CreatedAt = t.CreatedAt
                          };

            // Filtering
            if (request.SourceLocationId.HasValue)
                dbQuery = dbQuery.Where(x => x.SourceLocationId == request.SourceLocationId.Value);

            if (request.DestinationLocationId.HasValue)
                dbQuery = dbQuery.Where(x => x.DestinationLocationId == request.DestinationLocationId.Value);

            if (request.Status.HasValue)
            {
                var statusStr = request.Status.Value.ToString();
                dbQuery = dbQuery.Where(x => x.Status == statusStr);
            }

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderByDescending(x => x.CreatedAt);
            }

            return await sortedQuery.ToPagedListAsync<Response, Response>(request, ct);
        }
    }
}
