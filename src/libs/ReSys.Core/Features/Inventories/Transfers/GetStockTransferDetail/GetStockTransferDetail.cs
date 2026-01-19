using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Features.Inventories.Transfers.Common;

namespace ReSys.Core.Features.Inventories.Transfers.GetStockTransferDetail;

public static class GetStockTransferDetail
{
    public record Request(Guid Id);
    public record Response : StockTransferDetail;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var transfer = await (from t in context.Set<StockTransfer>()
                                  join src in context.Set<StockLocation>() on t.SourceLocationId equals src.Id
                                  join dest in context.Set<StockLocation>() on t.DestinationLocationId equals dest.Id
                                  where t.Id == query.Request.Id
                                  select new Response
                                  {
                                      Id = t.Id,
                                      ReferenceNumber = t.ReferenceNumber,
                                      SourceLocationId = t.SourceLocationId,
                                      SourceLocationName = src.Name,
                                      DestinationLocationId = t.DestinationLocationId,
                                      DestinationLocationName = dest.Name,
                                      Status = t.Status.ToString(),
                                      Reason = t.Reason,
                                      CreatedAt = t.CreatedAt,
                                      Items = (from item in t.Items
                                               join v in context.Set<Variant>() on item.VariantId equals v.Id
                                               select new StockTransferItemModel
                                               {
                                                   VariantId = item.VariantId,
                                                   Sku = v.Sku ?? "N/A",
                                                   VariantName = v.Product.Name,
                                                   Quantity = item.Quantity
                                               }).ToList()
                                  }).FirstOrDefaultAsync(ct);

            if (transfer == null)
                return StockTransferErrors.NotFound(query.Request.Id);

            return transfer;
        }
    }
}
