using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Admin.Inventories.Stocks.Common;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.GetStockItemDetail;

public static class GetStockItemDetail
{
    public record Request(Guid Id);
    public record Response : StockItemDetail;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var stockItem = await context.Set<StockItem>()
                .Include(x => x.Variant).ThenInclude(v => v.Product)
                .Include(x => x.StockLocation)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Request.Id && !x.IsDeleted, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(query.Request.Id);

            return stockItem.Adapt<Response>();
        }
    }
}
