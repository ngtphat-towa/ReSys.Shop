using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Features.Admin.Inventories.Locations.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Inventories.Locations.GetStockLocationsPagedList;

public static class GetStockLocationsPagedList
{
    public record Request : QueryOptions;
    public record Response : StockLocationListItem;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<StockLocation>()
                .Where(x => !x.IsDeleted)
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name);
            }

            return await sortedQuery.ToPagedListAsync<StockLocation, Response>(request, ct);
        }
    }
}
