using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.OptionTypes.GetOptionTypeSelectList;

public static class GetOptionTypeSelectList
{
    // Request: Inherits all query options
    public record Request : QueryOptions;

    // Response: Slice-specific alias for the select list item
    public record Response : OptionTypeSelectListItem;

    // Query: Returns a paged list (or all) of our specific Response type
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

            // Default sort for select lists is usually Name
            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Name);
            }

            // Finalize using the "ToPagedOrAllAsync" mode for select lists
            return await sortedQuery.ToPagedOrAllAsync<OptionType, Response>(
                request,
                cancellationToken);
        }
    }
}
