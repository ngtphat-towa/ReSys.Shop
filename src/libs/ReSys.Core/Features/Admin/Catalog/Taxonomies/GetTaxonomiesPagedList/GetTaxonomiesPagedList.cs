using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.GetTaxonomiesPagedList;

public static class GetTaxonomiesPagedList
{
    // Request:
    public record Request : QueryOptions;

    // Response:
    public record Response : TaxonomyListItem;

    // Query:
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Taxonomy>()
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

            return await sortedQuery.ToPagedListAsync<Taxonomy, Response>(
                request,
                cancellationToken);
        }
    }
}
