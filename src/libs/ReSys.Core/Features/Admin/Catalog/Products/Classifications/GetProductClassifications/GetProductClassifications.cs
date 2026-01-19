using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Admin.Catalog.Products.Classifications.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Catalog.Products.Classifications.GetProductClassifications;

public static class GetProductClassifications
{
    public record Request : QueryOptions
    {
        public Guid? ProductId { get; set; }
        public Guid? TaxonId { get; set; }
    }

    public record Response : ProductClassificationListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Classification>()
                .Include(x => x.Taxon)
                .AsNoTracking();

            if (request.ProductId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.ProductId == request.ProductId.Value);
            }

            if (request.TaxonId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.TaxonId == request.TaxonId.Value);
            }

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Position);
            }

            return await sortedQuery.ToPagedListAsync<Classification, Response>(
                request,
                cancellationToken);
        }
    }
}
