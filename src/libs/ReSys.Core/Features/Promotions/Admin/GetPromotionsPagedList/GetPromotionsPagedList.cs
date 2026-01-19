using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Features.Promotions.Admin.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Promotions.Admin.GetPromotionsPagedList;

public static class GetPromotionsPagedList
{
    public record Request : QueryOptions;
    public record Response : PromotionResponse;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await context.Set<Promotion>()
                .AsNoTracking()
                .ApplyFilter(query.Request)
                .ApplySearch(query.Request)
                .ApplySort(query.Request)
                .ToPagedListAsync<Promotion, Response>(query.Request, ct);
        }
    }
}