using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Transfers.ReceiveStockTransfer;

public static class ReceiveStockTransfer
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var transfer = await context.Set<StockTransfer>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (transfer == null)
                return StockTransferErrors.NotFound(command.Id);

            // 1. Mark as received in Domain (Workflow Guard)
            var receiveResult = transfer.Receive();
            if (receiveResult.IsError) return receiveResult.Errors;

            // 2. Physical Addition: Add stock to Destination Location
            foreach (var item in transfer.Items)
            {
                var stockItem = await context.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.VariantId == item.VariantId && x.StockLocationId == transfer.DestinationLocationId, ct);

                // If no stock item exists at destination, create it
                if (stockItem == null)
                {
                    var variant = await context.Set<ReSys.Core.Domain.Catalog.Products.Variants.Variant>()
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId, ct);
                    
                    var sku = variant?.Sku ?? $"VAR-{item.VariantId.ToString()[..8]}";
                    
                    var createResult = StockItem.Create(item.VariantId, transfer.DestinationLocationId, sku, 0);
                    if (createResult.IsError) return createResult.Errors;
                    
                    stockItem = createResult.Value;
                    context.Set<StockItem>().Add(stockItem);
                }

                var adjustResult = stockItem.AdjustStock(
                    item.Quantity, 
                    StockMovementType.Transfer, 
                    0, 
                    $"Transfer In: {transfer.ReferenceNumber}", 
                    transfer.Id.ToString());

                if (adjustResult.IsError) return adjustResult.Errors;
                
                context.Set<StockItem>().Update(stockItem);
            }

            context.Set<StockTransfer>().Update(transfer);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
