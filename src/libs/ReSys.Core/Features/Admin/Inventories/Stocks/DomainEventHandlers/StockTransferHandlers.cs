using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.DomainEventHandlers;

public static class StockTransferHandlers
{
    /// <summary>
    /// Deducts stock from the source location when a transfer is shipped.
    /// </summary>
    public class ShippedHandler(IApplicationDbContext context, ILogger<ShippedHandler> logger) 
        : INotificationHandler<StockTransferEvents.StockTransferShipped>
    {
        public async Task Handle(StockTransferEvents.StockTransferShipped notification, CancellationToken ct)
        {
            var transfer = notification.Transfer;
            logger.LogInformation("Processing stock deduction for Shipped Transfer {TransferId}", transfer.Id);

            foreach (var item in transfer.Items)
            {
                var stockItem = await context.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.StockLocationId == transfer.SourceLocationId && x.VariantId == item.VariantId, ct);

                if (stockItem == null)
                {
                    logger.LogError("Source stock record not found for Variant {VariantId} at Location {LocationId}", item.VariantId, transfer.SourceLocationId);
                    continue;
                }

                var result = stockItem.AdjustStock(
                    -item.Quantity, 
                    StockMovementType.TransferOut, 
                    reason: $"Transfer {transfer.ReferenceNumber} Shipped",
                    reference: transfer.Id.ToString());

                if (result.IsError)
                {
                    logger.LogError("Failed to deduct stock: {Error}", result.FirstError.Description);
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Adds stock to the destination location when a transfer is received.
    /// </summary>
    public class ReceivedHandler(IApplicationDbContext context, ILogger<ReceivedHandler> logger) 
        : INotificationHandler<StockTransferEvents.StockTransferReceived>
    {
        public async Task Handle(StockTransferEvents.StockTransferReceived notification, CancellationToken ct)
        {
            var transfer = notification.Transfer;
            logger.LogInformation("Processing stock addition for Received Transfer {TransferId}", transfer.Id);

            foreach (var item in transfer.Items)
            {
                var stockItem = await context.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.StockLocationId == transfer.DestinationLocationId && x.VariantId == item.VariantId, ct);

                if (stockItem == null)
                {
                    // Create stock record at destination if missing
                    var variant = await context.Set<Variant>().FindAsync([item.VariantId], ct);
                    if (variant == null) continue;

                    var createResult = StockItem.Create(item.VariantId, transfer.DestinationLocationId, variant.Sku ?? "UNKNOWN", initialStock: 0);
                    if (createResult.IsError) continue;
                    
                    stockItem = createResult.Value;
                    context.Set<StockItem>().Add(stockItem);
                }

                var result = stockItem.AdjustStock(
                    item.Quantity, 
                    StockMovementType.TransferIn, 
                    reason: $"Transfer {transfer.ReferenceNumber} Received",
                    reference: transfer.Id.ToString());

                if (result.IsError)
                {
                    logger.LogError("Failed to add stock: {Error}", result.FirstError.Description);
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
