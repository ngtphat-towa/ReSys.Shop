using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Catalog.OptionTypes.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;
using OptionType = ReSys.Core.Domain.Catalog.OptionTypes.OptionType;

namespace ReSys.Core.Features.Catalog.Products.OptionTypes.GetProductOptionTypes;

public static class GetProductOptionTypes
{
    public record Request : QueryOptions
    {
        public Guid ProductId { get; set; }
    }

    public record Response : OptionTypeSelectListItem;

    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Product>()
                .Where(p => p.Id == request.ProductId)
                .SelectMany(p => p.OptionTypes)
                .AsNoTracking();

            dbQuery = dbQuery
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Position);
            }

            return await sortedQuery.ToPagedListAsync<OptionType, Response>(
                request,
                cancellationToken);
        }
    }
}