using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Features.Admin.Catalog.Products.Images.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.GetProductImages;

public static class GetProductImages
{
    public record Request : QueryOptions
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
    }

    public record Response : ProductImageListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<ProductImage>()
                .Where(x => x.ProductId == request.ProductId)
                .AsNoTracking();

            if (request.VariantId.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.VariantId == request.VariantId.Value);
            }

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Position);
            }

            return await sortedQuery.ToPagedListAsync<ProductImage, Response>(
                request,
                cancellationToken);
        }
    }
}
