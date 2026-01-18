using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Features.Catalog.Products.Variants.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.Products.Variants.GetVariantsPagedList;

public static class GetVariantsPagedList
{
    public record Request : QueryOptions
    {
        public Guid? ProductId { get; set; }
    }

    public record Response : VariantListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Variant>()
                .Include(v => v.Product)
                .Include(v => v.OptionValues).ThenInclude(ov => ov.OptionType)
                .Where(v => !v.IsDeleted)
                .AsNoTracking();

            if (request.ProductId.HasValue)
            {
                dbQuery = dbQuery.Where(v => v.ProductId == request.ProductId.Value);
            }

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Position);
            }

            return await sortedQuery.ToPagedListAsync<Variant, Response>(
                request,
                cancellationToken);
        }
    }
}
