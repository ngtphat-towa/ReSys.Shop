using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products.Properties;
using ReSys.Core.Features.Admin.Catalog.Products.Properties.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Catalog.Products.Properties.GetProductProperties;

public static class GetProductProperties
{
    public record Request : QueryOptions
    {
        public Guid ProductId { get; set; }
    }

    public record Response : ProductPropertyListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<ProductProperty>()
                .Include(x => x.PropertyType)
                .AsNoTracking()
                .Where(x => x.ProductId == request.ProductId)
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.PropertyType.Position);
            }

            return await sortedQuery.ToPagedListAsync<ProductProperty, Response>(
                request,
                cancellationToken);
        }
    }
}
