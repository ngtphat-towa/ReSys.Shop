using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypesPagedList;

public static class GetOptionTypesPagedList
{
    // Request: Inherits all query options
    public record Request : QueryOptions;

    // Response: Slice-specific alias for the list item
    public record Response : OptionTypeListItem;

    // Query: Returns a paged list of our specific Response type
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<OptionType>()
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery
                    .OrderBy(x => x.Position)
                    .ThenBy(x => x.Name);
            }

            // Project directly to our Response type using Mapster
            return await sortedQuery.ToPagedListAsync<OptionType, Response>(
                request,
                cancellationToken);
        }
    }
}
