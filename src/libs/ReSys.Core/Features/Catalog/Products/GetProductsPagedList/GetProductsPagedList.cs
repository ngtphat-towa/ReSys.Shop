using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Catalog.Products.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.Products.GetProductsPagedList;

public static class GetProductsPagedList
{
    // Request:
    public record Request : QueryOptions;

    // Response:
    public record Response : ProductListItem;

    // Query:
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Product>()
                .Include(x => x.Variants)
                .Include(x => x.Images)
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery
                    .OrderBy(x => x.Name);
            }

            return await sortedQuery.ToPagedListAsync<Product, Response>(
                request,
                cancellationToken);
        }
    }
}
